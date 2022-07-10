using System;
using UnityEngine;

public class SetEntityCollideableInstruction : Instruction
{
    public override int getID() => 12;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        bool is_collideable = data[4] == 1;

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // tell entity manager to apply a mesh collider
        entity.GetComponent<EntityManager>().setCollideable(is_collideable);

        /*// check if entity has a mesh collider
        MeshCollider meshCollider = entity.GetComponent<MeshCollider>();

        // if should be collideable, make sure they have a mesh collider that is setup
        if (is_collideable && meshCollider == null)
        {
            meshCollider = entity.AddComponent<MeshCollider>();
            MeshFilter filter = entity.GetComponent<MeshFilter>();
            if (filter.mesh.vertexCount == 0)
            {
                foreach (MeshAssetManager.WaitingForMesh waiting in manager.assetPacketHandler.meshAssetManager.waitingForMesh)
                {
                    if (waiting.entityManager.meshFilter == filter)
                        waiting.entityManager.collider = meshCollider;
                }
            }
            else
                meshCollider.sharedMesh = filter.mesh;
        }
        // otherwise, make sure they do not have a mesh collider
        else if (!is_collideable && meshCollider != null)
            manager.DestroyUnityObject(meshCollider);*/

    }
}
public class CreateParticleEmitterInstruction : Instruction
{
    public override int getID() => 11;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int texture_id = BitConverter.ToInt32(data, 0);
        Vector3 emitter_position = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
        float emitter_lifetime_seconds = BitConverter.ToSingle(data, 16);
        float particles_rate = BitConverter.ToSingle(data, 20);
        float particle_duration = BitConverter.ToSingle(data, 24);
        float particle_speed = BitConverter.ToSingle(data, 28);
        Vector3 particle_direction_scale = new Vector3(BitConverter.ToSingle(data, 32), BitConverter.ToSingle(data, 36), BitConverter.ToSingle(data, 40));
        float particle_size = BitConverter.ToSingle(data, 44);

        // create object and get components
        GameObject emitter = GameObject.Instantiate(manager.baseParticle, emitter_position, Quaternion.identity);
        ParticleSystem particleSystem = emitter.GetComponent<ParticleSystem>();
        DestroyAfterTime destroyer = emitter.GetComponent<DestroyAfterTime>();

        // set kill time
        destroyer.destroySeconds = emitter_lifetime_seconds;

        // set particle
        particleSystem.startLifetime = particle_duration;
        particleSystem.startSpeed = particle_speed;
        particleSystem.startSize = particle_size;
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rate = particles_rate;
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = particle_direction_scale;

        // set particle texture
        manager.assetPacketHandler.textureAssetManager.setTexture(manager, emitter.GetComponent<ParticleSystemRenderer>(), texture_id);
    }
}
public class CreateLightInstruction : Instruction
{
    public override int getID() => 13;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        Vector3 lightPosition = new Vector3(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8));
        Vector3 lightRotation = new Vector3(BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20));
        byte lightType = data[24];
        float lightIntensity = BitConverter.ToSingle(data, 25);
        float lightSize = BitConverter.ToSingle(data, 29);
        float lightSpotAngle = BitConverter.ToSingle(data, 33);
        Color lightColor = new Color(((float)data[37]) / 255f, ((float)data[38]) / 255f, ((float)data[39]) / 255f);

        // create new light
        GameObject lightObject = GameObject.Instantiate(manager.baseLight, lightPosition, Quaternion.Euler(lightRotation));
        Light light = lightObject.GetComponent<Light>();

        // update light type
        if (lightType == 0x00)
            light.type = LightType.Directional;
        else if (lightType == 0x01)
            light.type = LightType.Spot;
        else if (lightType == 0x02)
            light.type = LightType.Point;
        else
        {
            Debug.LogError("Unknown light type " + lightType);
            return;
        }

        // update light component
        light.intensity = lightIntensity;
        light.range = lightSize;
        light.spotAngle = lightSpotAngle;
        light.color = lightColor;
    }
}