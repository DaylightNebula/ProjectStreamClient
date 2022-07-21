using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public class CreatePointFromRaycastInstruction: Instruction
{
    string entityName;
    string pointName;
    Vector3 rotation;
    float maxDistance;
    string pointDirection;

    public CreatePointFromRaycastInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
        pointName = xml.Attributes["point"].Value;
        rotation = XMLDecoder.decodeVector(xml.Attributes["rotation"], new Vector3(0f, 0f, 0f));
        maxDistance = XMLDecoder.decodeFloat(xml.Attributes["max_distance"], 100f);
        pointDirection = XMLDecoder.decodeString(xml.Attributes["point_direciton"], "ray_direction");
    }

    public override void execute(Manager manager)
    {
        GameObject entity = XMLDecoder.getEntityWithKeywords(manager, entityName);

        // prepare ray
        Vector3 rayDirection = entity.transform.TransformDirection(Vector3.forward) + rotation;
        RaycastHit hit;

        // point data
        Vector3 point = entity.transform.position + (rayDirection * maxDistance);
        Vector3 pointRotation = Vector3.down;

        // create raycast point
        if (Physics.Raycast(
            entity.transform.position,
            rayDirection,
            out hit,
            maxDistance)
        )
        {
            // if hit, create point at location with proper rotation
            point = hit.point;
            if (pointDirection == "ray_direction")
                pointRotation = rayDirection;
            else if (pointDirection == "face_direction")
                pointRotation = hit.normal;
        }

        // handle point directions 2-6
        if (pointDirection == "Z+")
            pointRotation = Vector3.forward;
        else if (pointDirection == "Z-")
            pointRotation = Vector3.back;
        else if (pointDirection == "X+")
            pointRotation = Vector3.right;
        else if (pointDirection == "X-")
            pointRotation = Vector3.left;
        else if (pointDirection == "Y+")
            pointRotation = Vector3.up;

        // save point and its rotation
        manager.UpdatePointLocation(pointName, point, pointRotation);
    }
}
public class MoveEntityToPointInstruction: Instruction
{
    string entityName;
    string pointName;
    bool usePointDirection;
    Vector3 rotationOffset;

    public MoveEntityToPointInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        entityName = xml.Attributes["entity"].Value;
        pointName = xml.Attributes["point"].Value;
        usePointDirection = XMLDecoder.decodeBoolean(xml.Attributes["use_point_direction"], true);
        rotationOffset = XMLDecoder.decodeVector(xml.Attributes["rotation_offset"], new Vector3(0f, 0f, 0f));
    }

    public override void execute(Manager manager)
    {
        // safety stuff
        if (!manager.entities.ContainsKey(entityName)) return;
        if (!manager.DoesPointExist(pointName)) return;

        // get entity and point pair
        GameObject entity = manager.entities[entityName];
        KeyValuePair<Vector3, Vector3> pointPair = manager.GetPointLocation(pointName);

        // create rotation
        Vector3 rotation = rotationOffset;
        if (usePointDirection) rotation += pointPair.Value;

        // set entities position and rotation
        entity.transform.SetPositionAndRotation(pointPair.Key, Quaternion.Euler(rotation));
    }
}
