using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorPacketHandler
{
    public Manager manager;
    BehaviorClient client;

    public BehaviorPacketHandler(Manager manager)
    {
        this.manager = manager;
    }

    public void setClient(BehaviorClient client)
    {
        this.client = client;
    }

    public void processPacket(int id, byte[] data)
    {
        switch (id) {
            case 0: // hello packet
                Debug.LogWarning("We should not have gotten a hello packet!");
                break;
            case 1: // Connect to asset server packet
                // get port
                int port = BitConverter.ToInt32(data, 0);

                // get rid of the first 4 bytes of the data.  Yes I know this is inefficient as fuck
                Array.Reverse(data);
                Array.Resize(ref data, data.Length - 4);
                Array.Reverse(data);

                // get address
                string address = System.Text.Encoding.UTF8.GetString(data);

                // tell the manager to connect
                manager.ConnectToAssetServer(address, port);
                break;
            case 2: break;
            case 3:
                // unpack
                byte fileType = data[0];
                int fileLength = BitConverter.ToInt32(data, 1);
                string fileText = System.Text.Encoding.UTF8.GetString(data).Substring(5, fileLength);

                // call decode
                manager.xmlDecoder.decode(fileType, fileText);

                // end
                break;
            default:
                Debug.LogWarning("No packet response was created for packet ID " + id);
                break;
        }
    }
}
