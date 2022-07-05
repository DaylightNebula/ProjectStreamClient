using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AssetManager
{
    public abstract int getAssetID();
    public abstract int getPacketID();
    public abstract void ProcessData(Manager manager, byte[] data);
    public abstract void Request(Manager manager, int id);
}
