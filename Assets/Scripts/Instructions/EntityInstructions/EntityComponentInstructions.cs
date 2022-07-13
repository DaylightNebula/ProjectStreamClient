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
    }
}
public class CreateParticleEmitterInstruction : Instruction
{
    public override int getID() => 11;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        int texture_id = BitConverter.ToInt32(data, 4);
        float emitter_lifetime_seconds = BitConverter.ToSingle(data, 8);
        float particles_rate = BitConverter.ToSingle(data, 12);
        float particle_duration = BitConverter.ToSingle(data, 16);
        float particle_speed = BitConverter.ToSingle(data, 20);
        Vector3 particle_direction_scale = new Vector3(BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32));
        float particle_size = BitConverter.ToSingle(data, 36);

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // create object and get components
        ParticleSystem particleSystem = entity.AddComponent<ParticleSystem>();
        DestroyAfterTime destroyer = entity.AddComponent<DestroyAfterTime>();

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
        manager.assetPacketHandler.textureAssetManager.setTexture(manager, entity.AddComponent<ParticleSystemRenderer>(), texture_id);
    }
}
public class CreateLightInstruction : Instruction
{
    public override int getID() => 13;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        byte lightType = data[4];
        float lightIntensity = BitConverter.ToSingle(data, 5);
        float lightSize = BitConverter.ToSingle(data, 9);
        float lightSpotAngle = BitConverter.ToSingle(data, 13);
        Color lightColor = new Color(((float)data[17]) / 255f, ((float)data[18]) / 255f, ((float)data[19]) / 255f);

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // create new light on entity
        Light light = entity.AddComponent<Light>();

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
public class SetMeshInstruction : Instruction
{
    public override int getID() => 14;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        int mesh_id = BitConverter.ToInt32(data, 4);

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // add mesh to entity manager
        entity.GetComponent<EntityManager>().addMesh(mesh_id);
    }
}
public class SetMaterialInstruction : Instruction
{
    public override int getID() => 15;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        int material_id = BitConverter.ToInt32(data, 4);

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // add mesh to entity manager
        entity.GetComponent<EntityManager>().addMaterial(material_id);
    }
}
public class SetRigidbodyInstruction : Instruction
{
    public override int getID() => 16;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        bool use_rigidbody = data[4] == 1;
        float rb_mass = BitConverter.ToSingle(data, 5);
        float rb_drag = BitConverter.ToSingle(data, 9);
        float rb_angular_drag = BitConverter.ToSingle(data, 13);
        bool rb_use_gravity = data[17] == 1;
        bool rb_is_kinematic = data[18] == 1;

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // set rigidbody
        entity.GetComponent<EntityManager>().setRigidbody(use_rigidbody, rb_mass, rb_drag, rb_angular_drag, rb_use_gravity, rb_is_kinematic);
    }
}
public class ApplyForceToRigidbodyInstruction : Instruction
{
    public override int getID() => 17;

    public override void execute(Manager manager, byte[] data)
    {
        // unpack
        int entity_id = BitConverter.ToInt32(data, 0);
        Vector3 force = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));

        // try to get entity
        if (!manager.entities.ContainsKey(entity_id)) return;
        GameObject entity = manager.entities[entity_id];

        // try to apply force to the entities rigid body
        EntityManager entityManager = entity.GetComponent<EntityManager>();
        if (entityManager.rigidbody != null) entityManager.rigidbody.AddForce(force, ForceMode.Impulse);
    }
}