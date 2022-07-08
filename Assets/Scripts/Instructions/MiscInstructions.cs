using System;

public class CallEventInstruction : Instruction
{
    public override int getID() => 5;
    public override void execute(Manager manager, byte[] data)
    {
        // break if behavior client does not exist
        if (manager.behaviorClient == null) return;

        // send packet
        manager.behaviorClient.sendPacket(6, data);
    }
}
public class LoadAssetInstruction : Instruction
{
    public override int getID() => 9;
    public override void execute(Manager manager, byte[] data)
    {                
        // unpack
        int asset_id = BitConverter.ToInt32(data, 0);
        byte asset_type_id = data[4];

        // get asset manager and tell it to request asset_id
        manager.assetPacketHandler.assetManagers[asset_type_id].Request(manager, asset_id);
    }
}