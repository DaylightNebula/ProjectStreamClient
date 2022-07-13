using System;
using UnityEngine;

public class CreateRaycastPointInstruction : Instruction
{
    public override int getID() => 3;
    public override void execute(Manager manager, byte[] data)
    {
        // unpack data
        int point_id = BitConverter.ToInt32(data, 0);
        Vector3 rotationOffset = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
        float maxRayDistance = BitConverter.ToSingle(data, 16);
        int point_direction_id = data[20];
        bool rightController = data[21] == 1; // false for left, true for right

        // get controller
        GameObject controller = null;
        if (rightController) controller = manager.rightController;
        else controller = manager.leftController;

        // prepare ray
        Vector3 rayDirection = controller.transform.TransformDirection(Vector3.forward) + rotationOffset;
        RaycastHit hit;

        // point data
        Vector3 point = controller.transform.position + (rayDirection * maxRayDistance);
        Vector3 pointRotation = Vector3.down;

        // create raycast point
        if (Physics.Raycast(
            controller.transform.position,
            rayDirection,
            out hit,
            maxRayDistance)
        )
        {
            // if hit, create point at location with proper rotation
            point = hit.point;
            if (point_direction_id == 0)
                pointRotation = rayDirection;
            else if (point_direction_id == 1)
                pointRotation = hit.normal;
        }

        // handle point directions 2-6
        if (point_direction_id == 2)
            pointRotation = Vector3.forward;
        else if (point_direction_id == 3)
            pointRotation = Vector3.back;
        else if (point_direction_id == 4)
            pointRotation = Vector3.right;
        else if (point_direction_id == 5)
            pointRotation = Vector3.left;
        else if (point_direction_id == 6)
            pointRotation = Vector3.up;

        // save point and its rotation
        manager.UpdatePointLocation(point_id, point, pointRotation);
    }
}
public class MoveEntityToPointInstruction : Instruction
{
    public override int getID() => 4;
    public override void execute(Manager manager, byte[] data)
    {
        // unpack data
        int entity_id = BitConverter.ToInt32(data, 0);
        int point_id = BitConverter.ToInt32(data, 4);
        bool usePointDirection = data[8] == 1;
        Vector3 rotationOffset = new Vector3(BitConverter.ToSingle(data, 9), BitConverter.ToSingle(data, 13), BitConverter.ToSingle(data, 17));

        // if neither entities or point maps have keys, throw error and break
        if (!manager.entities.ContainsKey(entity_id) || !manager.DoesPointExist(point_id))
        {
            Debug.LogWarning("Either point " + point_id + " or entity " + entity_id + " does not exist!");
            return;
        }

        // get point and entity
        GameObject entity = manager.entities[entity_id];
        var pointPair = manager.GetPointLocation(point_id);

        // teleport entity
        Quaternion rotation = Quaternion.identity;
        if (usePointDirection) rotation = Quaternion.Euler(pointPair.Value + rotationOffset);
        entity.transform.SetPositionAndRotation(pointPair.Key, rotation);
    }
}
public class CreatePointFromMidPointOfTwoPointsInstruction : Instruction
{
    public override int getID() => 6;
    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int point_id_a = BitConverter.ToInt32(data, 0);
        int point_id_b = BitConverter.ToInt32(data, 4);
        int new_point_id = BitConverter.ToInt32(data, 8);

        // make sure point a and point b exist
        if (!manager.DoesPointExist(point_id_a) || !manager.DoesPointExist(point_id_b)) return;

        // get points
        var point_a = manager.GetPointLocation(point_id_a);
        var point_b = manager.GetPointLocation(point_id_b);

        // create new point
        Vector3 midpoint = (point_a.Key + point_b.Key) / 2;
        Vector3 midpoint_rotation = (point_a.Value + point_b.Value) / 2;
        manager.UpdatePointLocation(new_point_id, midpoint, midpoint_rotation);
    }
}