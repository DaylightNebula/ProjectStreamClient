using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundFromEntityInstruction : Instruction
{
    string entityName;
    string soundName;
    float speed;
    float volume;
    bool wait;

    public PlaySoundFromEntityInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        // get mandatory stuff
        entityName = xml.Attributes["entity"].Value;
        soundName = xml.Attributes["sound"].Value;

        // get not mandatory stuff
        speed = XMLDecoder.decodeFloat(xml.Attributes["speed"], 1f);
        volume = XMLDecoder.decodeFloat(xml.Attributes["volume"], 1f);
        wait = XMLDecoder.decodeBoolean(xml.Attributes["wait_for_download"], true);
    }

    public override void execute(Manager manager, GameObject root)
    {
        if (!manager.entities.ContainsKey(entityName)) return;
        GameObject entity = manager.entities[entityName];

        manager.assetPacketHandler.soundAssetManager.playSoundFromObject(manager, entity, soundName, volume, wait);
        entity.GetComponent<EntityManager>();
    }
}
