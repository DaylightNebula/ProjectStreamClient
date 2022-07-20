using NativeWebSocket;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class AssetClient
{
    AssetPacketHandler packetHandler;

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

    public AssetClient(AssetPacketHandler handler)
    {
        this.packetHandler = handler;
    }

    // Start the client
    List<byte[]> waitingSends = new List<byte[]>();
    public async void start(string address, int port)
    {
        try
        {
            // create tcp client
            string targetAddress = "ws://" + address + ":" + port;
            if (Application.isEditor) targetAddress = "ws://localhost:" + port;
            socket = new WebSocket(targetAddress);
            Debug.Log("Starting asset client!" + targetAddress);

            // some basic callbacks for websocket
            socket.OnOpen += () =>
            {
                Debug.Log("Connection open!");
                foreach (byte[] bytes in waitingSends)
                    socket.Send(bytes);
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
                byte packetID = bytes[0];
                int packetLength = BitConverter.ToInt32(bytes, 1);
                byte[] data = new byte[packetLength];
                Buffer.BlockCopy(bytes, 5, data, 0, packetLength);

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
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    // function to send a packet
    public void sendPacket(byte packetID, byte[] data)
    {
        int[] ints = new int[] { data.Length };
        byte[] packet = new byte[5 + data.Length];
        packet[0] = packetID;
        Buffer.BlockCopy(ints, 0, packet, 1, 4);
        Buffer.BlockCopy(data, 0, packet, 5, data.Length);

        if (socket.State == WebSocketState.Open)
            socket.Send(packet);
        else
            waitingSends.Add(packet);
    }

    public void update()
    {
        // receive messages
#if !UNITY_WEBGL || UNITY_EDITOR
        socket.DispatchMessageQueue();
#endif

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
        socket.Close();
    }
}
