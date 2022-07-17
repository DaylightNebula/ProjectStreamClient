using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class ApplyForceToRigidbodyInstruction : Instruction
{
    string entityName;
    Vector3 force;

    public ApplyForceToRigidbodyInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {

        entityName = xml.Attributes["entity"].Value;
        force = XMLDecoder.decodeVector(xml.Attributes["force"], new Vector3(0f, 0f, 0f));
    }

    public override void execute(Manager manager, GameObject root)
    {
        if (!manager.entities.ContainsKey(entityName)) return;
        GameObject entity = manager.entities[entityName];
        entity.GetComponent<EntityManager>().rigidbody.AddForce(force, ForceMode.Impulse);
    }
}
