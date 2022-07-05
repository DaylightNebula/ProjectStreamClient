using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAssetManager : AssetManager
{
    public override int getAssetID() => 3;
    public override int getPacketID() => 8;

    public override void ProcessData(Manager manager, byte[] data)
    {
    }

    public override void Request(Manager manager, int id)
    {
    }
}
