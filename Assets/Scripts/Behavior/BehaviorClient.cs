using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using NativeWebSocket;

public class BehaviorClient
{
    BehaviorPacketHandler packetHandler;

    // NETWORK VARS
    WebSocket socket;

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
    public async void start(string address, int port)
    {
        Debug.Log("Starting behavior client!");
        try
        {
            // create tcp client
            socket = new WebSocket("ws://" + address + ":" + port);

            // some basic callbacks for websocket
            socket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
            };

            socket.OnError += (e) =>
            {
                Debug.Log("Error! " + e);
            };

            socket.OnClose += (e) =>
            {
                Debug.Log("Connection closed!");
            };

            // receive callback
            socket.OnMessage += (bytes) =>
            {
                int packetID = BitConverter.ToInt32(bytes, 0);
                int packetLength = BitConverter.ToInt32(bytes, 0);
                byte[] data = new byte[packetLength];
                Buffer.BlockCopy(bytes, 8, data, 0, packetLength);

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
            };

            await socket.Connect();

            // send hello packet
            sendPacket(
                0x00, new byte[0]
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        /*var thread = new Thread(ThreadStart);
        thread.Start();*/
    }

    // function to send a packet
    public void sendPacket(byte packetID, byte[] data)
    {
        /*stream.WriteByte(packetID);
        byte[] size = BitConverter.GetBytes(data.Length);
        stream.Write(size, 0, size.Length);
        stream.Flush();
        stream.Write(data, 0, data.Length);
        stream.Flush();*/
        int[] ints = new int[] { packetID, data.Length };
        byte[] packet = new byte[8 + data.Length];
        Buffer.BlockCopy(ints, 0, packet, 0, 8);
        Buffer.BlockCopy(data, 0, packet, 8, data.Length);
        socket.Send(packet);
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

                // clear packet list
                packets.Clear();
            }
        }
    }

    public void dispose()
    {
        socket.Close();
    }
}
