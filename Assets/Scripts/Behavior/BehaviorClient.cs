using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class BehaviorClient
{
    BehaviorPacketHandler packetHandler;

    // NETWORK VARS
    TcpClient client;
    NetworkStream stream;

    // PACKET STUFF
    public struct Packet
    {
        public int packetID;
        public byte[] data;

        public Packet(int packetID, byte[] data)
        {
            this.packetID = packetID;
            this.data = data;
        }
    }
    public List<Packet> packets = new List<Packet>();

    public BehaviorClient(BehaviorPacketHandler handler)
    {
        this.packetHandler = handler;
    }

    // Start the client
    public void start(string address, int port)
    {
        Debug.Log("Starting behavior client!");
        try
        {
            // create tcp client
            client = new TcpClient(address, port);
            stream = client.GetStream();

            // send hello packet
            sendPacket(
                0x00, new byte[0]
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        var thread = new Thread(ThreadStart);
        thread.Start();
    }

    // function to send a packet
    public void sendPacket(byte packetID, byte[] data)
    {
        stream.WriteByte(packetID);
        byte[] size = BitConverter.GetBytes(data.Length);
        stream.Write(size, 0, size.Length);
        stream.Flush();
        stream.Write(data, 0, data.Length);
        stream.Flush();
    }

    // creates a loop on another thread to get packets from network stream
    bool runRecvLoop = true;
    public void ThreadStart()
    {
        while (runRecvLoop)
        {
            // check for available packets
            if (stream != null && stream.DataAvailable)
            {
                // get packet id
                byte[] idBytes = new byte[1];
                stream.Read(idBytes, 0, 1);
                int packetID = idBytes[0];

                // get packet size
                byte[] sizeBytes = new byte[4];
                stream.Read(sizeBytes, 0, 4);
                int packetSize = BitConverter.ToInt32(sizeBytes, 0);

                // get data
                /*byte[] data = new byte[packetSize];
                stream.Read(data, 0, packetSize);
                packetHandler.processPacket(packetID, data);*/
                byte[] data = new byte[packetSize];
                int counter = 0;
                do
                {
                    if (stream.DataAvailable)
                    {
                        data[counter] = (byte)stream.ReadByte();
                        counter++;
                    }
                } while (counter < packetSize);

                // save packet list
                lock (packets)
                {
                    packets.Add(
                        new Packet(
                            packetID,
                            data
                        )
                    );
                }
            }
        }
    }

    public void update()
    {
        // if avaiable packets
        if (packets.Count > 0)
        {
            lock (packets)
            {
                // process each packet
                foreach (Packet packet in packets)
                {
                    packetHandler.processPacket(packet.packetID, packet.data);
                }
            }

            // clear packet list
            packets.Clear();
        }
    }

    public void dispose()
    {
        // on quit, close the connection
        if (client != null) client.Close();
        runRecvLoop = false;
    }
}
