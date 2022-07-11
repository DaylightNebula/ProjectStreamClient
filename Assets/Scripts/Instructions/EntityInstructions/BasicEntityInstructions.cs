using System;
using UnityEngine;

public class CreateEntityInstruction : Instruction
{
    public override int getID() => 0;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack data
        int entity_id = BitConverter.ToInt32(data, 0);
        Vector3 position = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
        Quaternion rotation = Quaternion.Euler(BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24));
        Vector3 scale = new Vector3(BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32), BitConverter.ToSingle(data, 36));

        // create entity
        GameObject entity = GameObject.Instantiate(manager.baseObject, position, rotation);
        manager.entities.Add(entity_id, entity);

        // update entity manager
        entity.GetComponent<EntityManager>().manager = manager;

        // apply scale
        entity.transform.localScale = scale;
    }
}
public class RemoveEntityInstruction : Instruction
{
    public override int getID() => 1;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack data
        int entity_id = BitConverter.ToInt32(data, 0);

        // get entity
        GameObject entity = manager.entities[entity_id];

        // if does not exist, skip
        if (entity == null)
        {
            Debug.LogError("Entity with id " + entity_id + " does not exist!");
            return;
        }

        // destroy entity
        GameObject.Destroy(entity, 0f);

        // remove entity
        manager.entities.Remove(entity_id);
    }
}
public class UpdateEntityTransformInstruction : Instruction
{
    public override int getID() => 2;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack data
        int entity_id = BitConverter.ToInt32(data, 0);
        Vector3 position = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
        Quaternion rotation = Quaternion.Euler(BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24));
        Vector3 scale = new Vector3(BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32), BitConverter.ToSingle(data, 36));

        // get entity
        GameObject entity = manager.entities[entity_id];

        // if does not exist, skip
        if (entity == null)
        {
            Debug.LogError("Entity with id " + entity_id + " does not exist!");
            return;
        }

        // update entity transform
        entity.transform.SetPositionAndRotation(position, rotation);
        entity.transform.localScale = scale;

    }
}
public class MoveEntityWithTimeInstruction : Instruction
{
    public override int getID() => 8;

    public override void execute(Manager manager, byte[] data)
    {                
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        Vector3 xyz = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
        float timeToComplete = BitConverter.ToSingle(data, 16);
        bool isAdditive = data[20] == 1;

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // get target position
        Vector3 targetPosition = xyz;
        if (isAdditive) targetPosition += entity.transform.position;

        // if time to complete if greater than 0, run it over ttime
        if (timeToComplete > 0f)
        {
            MoveObjectToLocationOverTime move = entity.AddComponent<MoveObjectToLocationOverTime>();
            move.init(targetPosition, timeToComplete);
        }
        // otherwise, just update position
        else entity.transform.position = targetPosition;

    }
}
public class ChangeEntityVisibilityInstruction : Instruction
{
    public override int getID() => 7;

    public override void execute(Manager manager, byte[] data)
    {                
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        bool showing = data[4] == 1;

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // set visibility
        entity.SetActive(showing);

    }
}
public class PlaySoundFromEntityInstruction : Instruction
{
    public override int getID() => 10;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int sound_id = BitConverter.ToInt32(data, 0);
        int entity_id = BitConverter.ToInt32(data, 4);
        float sound_speed = BitConverter.ToSingle(data, 8);
        float sound_volume = BitConverter.ToSingle(data, 12);
        bool wait_for_download = data[16] == 1;

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // play sound
        manager.assetPacketHandler.soundAssetManager.playSoundFromObject(manager, entity, sound_id, sound_volume, wait_for_download);
    }
}