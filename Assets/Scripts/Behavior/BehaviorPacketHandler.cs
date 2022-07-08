using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorPacketHandler
{
    Manager manager;
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
            case 2: // instruction packet
                // get when execute stuff
                int whenExecuteID = data[0];
                int whenExecuteData = data[1];

                // get number of instructions
                int numberOfInstructions = BitConverter.ToInt32(data, 2);

                // create instruction array
                InstructionData[] instructions = new InstructionData[numberOfInstructions];

                // create counter to current byte
                int currentByte = 6;

                // loop through each instruction to unpack
                for (int i = 0; i < numberOfInstructions; i++)
                {
                    // get instruction header
                    int instructionID = data[currentByte];
                    int instructionSize = BitConverter.ToInt32(data, currentByte + 1);
                    currentByte += 5;

                    // get instruction bytes
                    byte[] instructionBytes = new byte[instructionSize];
                    Array.Copy(data, currentByte, instructionBytes, 0, instructionSize);
                    currentByte += instructionSize;

                    // create instruction
                    instructions[i] = new InstructionData(instructionID, instructionSize, instructionBytes);
                }

                // apply instruction list
                manager.instructionManager.applyInstructionList(whenExecuteID, whenExecuteData, instructions);

                // end
                break;
            default:
                Debug.LogWarning("No packet response was created for packet ID " + id);
                break;
        }
    }
}
