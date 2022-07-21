using System.Xml;
using UnityEngine;
using System.Text;
using System;

public class CallEventInstruction : Instruction
{
    string event_id;

    public CallEventInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        event_id = xml.Attributes["event"].Value;
    }

    public override void execute(Manager manager)
    {
        // break if behavior client does not exist
        if (manager.behaviorClient == null) return;

        // build packet data
        int[] idLength = new int[] { event_id.Length };
        byte[] id = Encoding.UTF8.GetBytes(event_id);
        byte[] packet = new byte[4 + event_id.Length];
        Buffer.BlockCopy(idLength, 0, packet, 0, 4);
        Buffer.BlockCopy(id, 0, packet, 4, id.Length);

        // send packet
        manager.behaviorClient.sendPacket(6, packet);
    }
}
