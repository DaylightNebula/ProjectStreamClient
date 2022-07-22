using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawnInstruction : Instruction
{
    XmlNode xml;

    public EntitySpawnInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        this.xml = xml;
    }

    public override void execute(Manager manager)
    {
        foreach (XmlNode entity in xml.ChildNodes)
            manager.xmlDecoder.decodeEntity(entity);
    }
}
public class EntityRemoveInstruction : Instruction
{
    string entityName;

    public EntityRemoveInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
    }

    public override void execute(Manager manager)
    {
        if (manager.entities.ContainsKey(entityName))
            manager.entities[entityName].GetComponent<EntityManager>().remove();
    }
}
public class EntitySetTransformInstruction : Instruction
{
    string entityName;
    Vector3 position;
    Vector3 rotation;
    Vector3 scale;

    public EntitySetTransformInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
        position = XMLDecoder.decodeVector(xml.Attributes["position"], new Vector3(0f, 0f, 0f));
        rotation = XMLDecoder.decodeVector(xml.Attributes["rotation"], new Vector3(0f, 0f, 0f));
        scale = XMLDecoder.decodeVector(xml.Attributes["scale"], new Vector3(1f, 1f, 1f));
    }

    public override void execute(Manager manager)
    {
        if (!manager.entities.ContainsKey(entityName)) return;
        GameObject entity = manager.entities[entityName];
        entity.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        entity.transform.localScale = scale;
    }
}
public class SetEntityActiveInstruction: Instruction
{
    string entityName;
    bool active;

    public SetEntityActiveInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
        active = XMLDecoder.decodeBoolean(xml.Attributes["active"], true);
    }

    public override void execute(Manager manager)
    {
        if (!manager.entities.ContainsKey(entityName)) return;
        GameObject entity = manager.entities[entityName];
        entity.SetActive(active);
    }
}
public class MoveEntityInstruction : Instruction
{
    string entityName;
    Vector3 xyz;
    float timeToMove;
    bool isAdditive;

    public MoveEntityInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
        xyz = XMLDecoder.decodeVector(xml.Attributes["xyz"], new Vector3(0f, 0f, 0f));
        timeToMove = XMLDecoder.decodeFloat(xml.Attributes["time_to_move"], 0f);
        isAdditive = XMLDecoder.decodeBoolean(xml.Attributes["isAdditive"], true);
    }

    public override void execute(Manager manager)
    {
        // try to get entity
        if (!manager.entities.ContainsKey(entityName)) return;
        GameObject entity = manager.entities[entityName];

        // get target position
        Vector3 targetPosition = xyz;
        if (isAdditive) targetPosition += entity.transform.position;

        // if time to complete if greater than 0, run it over ttime
        if (timeToMove > 0f)
        {
            MoveObjectToLocationOverTime move = entity.AddComponent<MoveObjectToLocationOverTime>();
            move.init(targetPosition, timeToMove);
        }
        // otherwise, just update position
        else entity.transform.position = targetPosition;
    }
}
public class SetEntityParticleEmitterInstruction: Instruction
{
    // mandatory stuff
    string entityID;
    bool enabled;

    // not mandatory stuff
    string texture;
    Vector3 directionScale;
    float lifetime;
    float duration;
    float speed;
    float size;
    float rate;

    public SetEntityParticleEmitterInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        // unpack mandatory
        entityID = xml.Attributes["entity"].Value;
        enabled = XMLDecoder.decodeBoolean(xml.Attributes["enabled"], true);

        // unpack not mandatory
        texture = XMLDecoder.decodeString(xml.Attributes["texture"], "");
        directionScale = XMLDecoder.decodeVector(xml.Attributes["direction_scale"], new Vector3(1f, 1f, 1f));
        lifetime = XMLDecoder.decodeFloat(xml.Attributes["lifetime"], 5f);
        duration = XMLDecoder.decodeFloat(xml.Attributes["duration"], 5f);
        speed = XMLDecoder.decodeFloat(xml.Attributes["speed"], 5f);
        size = XMLDecoder.decodeFloat(xml.Attributes["size"], 1f);
        rate = XMLDecoder.decodeFloat(xml.Attributes["rate"], 1f);
    }

    public override void execute(Manager manager)
    {
        // try to get entity
        if (!manager.entities.ContainsKey(entityID)) return;
        EntityManager entity = manager.entities[entityID].GetComponent<EntityManager>();

        // update particle emitter in entity
        entity.setParticleEmitter(enabled, directionScale, texture, lifetime, duration, speed, size, rate);
    }
}
public class CreateParticleBurstAtEntityInstruction : Instruction
{
    // mandatory stuff
    string entityID;
    bool enabled;

    // not mandatory stuff
    string texture;
    Vector3 directionScale;
    float count;
    float duration;
    float speed;
    float size;

    public CreateParticleBurstAtEntityInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        // unpack mandatory
        entityID = xml.Attributes["entity"].Value;
        enabled = XMLDecoder.decodeBoolean(xml.Attributes["enabled"], true);

        // unpack not mandatory
        texture = XMLDecoder.decodeString(xml.Attributes["texture"], "");
        directionScale = XMLDecoder.decodeVector(xml.Attributes["direction_scale"], new Vector3(1f, 1f, 1f));
        count = XMLDecoder.decodeFloat(xml.Attributes["lifetime"], 30f);
        duration = XMLDecoder.decodeFloat(xml.Attributes["duration"], 5f);
        speed = XMLDecoder.decodeFloat(xml.Attributes["speed"], 5f);
        size = XMLDecoder.decodeFloat(xml.Attributes["size"], 1f);
    }

    public override void execute(Manager manager)
    {
        // try to get entity
        if (!manager.entities.ContainsKey(entityID)) return;
        EntityManager entity = manager.entities[entityID].GetComponent<EntityManager>();

        // update particle emitter in entity
        entity.createParticleBurst(directionScale, texture, duration, count, speed, size);
    }
}