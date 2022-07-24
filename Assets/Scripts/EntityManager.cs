using System;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public Manager manager;

    public string name;
    public string mesh;
    public string material;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider collider;
    public Rigidbody rigidbody;
    public Light light;

    // particle stuffs
    ParticleSystemRenderer particleRenderer;

    Vector3 position;
    Quaternion rotation;
    Vector3 scale;
    bool isActive = true;

    // instruction stuffs
    Instruction[] onUpdateInstructions;
    Instruction[] onMoveInstructions;
    Instruction[] onRotateInstructions;
    Instruction[] onScaleInstructions;

    void FixedUpdate()
    {
        bool updateServer = false;

        // call update instructions
        if (onUpdateInstructions != null) Instruction.runInstructions(manager, onUpdateInstructions);

        // if move call instructions for that
        if (position != transform.position)
        {
            updateServer = true;
            if (onMoveInstructions != null) Instruction.runInstructions(manager, onMoveInstructions);
        }

        // if move call instructions for that
        if (rotation != transform.rotation)
        {
            updateServer = true;
            if (onRotateInstructions != null) Instruction.runInstructions(manager, onRotateInstructions);
        }

        // if move call instructions for that
        if (scale != transform.localScale)
        {
            updateServer = true;
            if (onScaleInstructions != null) Instruction.runInstructions(manager, onScaleInstructions);
        }

        // if we need to update the servers entity, do so
        if (updateServer)
            sendEntityTransformUpdate();

        // update trackers
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    void OnEnable()
    {
        isActive = false;
        sendEntityTransformUpdate();
    }

    void OnDisable()
    {
        isActive = true;
        sendEntityTransformUpdate();
    }

    void sendEntityTransformUpdate()
    {}

    public void remove()
    {
        Destroy(gameObject);
    }

    public void addMesh(string mesh)
    {
        // update ID and make sure everything needed for display exists
        this.mesh = mesh;
        addMeshFilterAndRenderer();

        // apply mesh
        manager.setMesh(this);
    }

    public void addMaterial(string material)
    {
        // update ID and make sure everything needed for display exists
        this.material = material;
        addMeshFilterAndRenderer();

        // apply material
        manager.setMaterial(this);
    }

    private void addMeshFilterAndRenderer()
    {
        // make sure mesh filter and mesh renderer exist and are stored in the variables
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    public void setCollideable(bool isCollideable)
    {
        if (isCollideable) makeCollideable();
        else removeCollideable();
    }

    private void makeCollideable()
    {
        if (collider == null)
            collider = gameObject.AddComponent<MeshCollider>();

        // if mesh with meshID exists, apply it right now
        if (meshFilter != null && meshRenderer != null && manager.meshes.ContainsKey(mesh))
            collider.sharedMesh = manager.meshes[mesh];
    }

    private void removeCollideable()
    {
        if (collider != null)
        {
            Destroy(collider);
            collider = null;
        }
    }

    public void setRigidbody(bool useRigidbody, float mass, float drag, float angularDrag, bool useGravity, bool isKinematic)
    {
        // if we should have a rigidbody and we dont have one, add one
        if (useRigidbody && rigidbody == null)
        {
            // set collider to convex to avoid rigidbody and collision errors (Unity bullshit)
            if (!collider.convex) collider.convex = true;

            // add rigidbody
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.mass = mass;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;
            rigidbody.useGravity = useGravity;
            rigidbody.isKinematic = isKinematic;
        }
        // otherise, if we shouldnt have a rigidbody and we have one, remove it
        else if (!useRigidbody && rigidbody != null)
        {
            // destroy rigidbody and make sure its tracking var is 0
            Destroy(rigidbody);
            rigidbody = null;
        }
    }

    public void setLight(bool enabled, string type, float intensity, float size, float angle, Color color)
    {
        // if light is not enabled, any light should be removed
        if (!enabled)
        {
            if (light != null) Destroy(light);
            return;
        }

        // make sure we have a light
        if (light == null)
            light = gameObject.AddComponent<Light>();

        // update light type
        if (type == "directional")
            light.type = LightType.Directional;
        else if (type == "spot")
            light.type = LightType.Spot;
        else if (type == "point")
            light.type = LightType.Point;
        else
        {
            Debug.LogError("Unknown light type " + type);
            return;
        }

        // update light component
        light.intensity = intensity;
        light.range = size;
        light.spotAngle = angle;
        light.color = color;
    }

    public void setParticleEmitter(bool enabled, Vector3 directionScale, string texture, float lifetime, float duration, float speed, float size, float rate)
    {
        // if particle emitter should not be enabled, any particle system or renderer must be removed
        if (!enabled)
        {
            // get particle system and renderer
            ParticleSystem system = gameObject.GetComponent<ParticleSystem>();

            // if they exist, destroy them
            if (system != null) Destroy(system);

            // cancel function
            return;
        }

        // create object and get components
        ParticleSystem particleSystem = gameObject.AddComponent<ParticleSystem>();
        DestroyAfterTime destroyer = gameObject.AddComponent<DestroyAfterTime>();

        // set kill time
        destroyer.objectToDestroy = particleSystem;
        destroyer.destroySeconds = lifetime;

        // set particle
        particleSystem.startLifetime = duration;
        particleSystem.startSpeed = speed;
        particleSystem.startSize = size;
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rate = rate;
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.scale = directionScale;

        // set particle texture
        if (particleRenderer == null)
        {
            gameObject.AddComponent<ParticleSystemRenderer>();
            particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
        }
        manager.assetPacketHandler.textureAssetManager.setTexture(manager, particleRenderer, texture);
    }

    public void createParticleBurst(Vector3 directionScale, string texture, float duration, float count, float speed, float size)
    {
        // if particle emitter should not be enabled, any particle system or renderer must be removed
        if (!enabled)
        {
            // get particle system and renderer
            ParticleSystem system = gameObject.GetComponent<ParticleSystem>();

            // if they exist, destroy them
            if (system != null) Destroy(system);

            // cancel function
            return;
        }

        // create object and get components
        ParticleSystem particleSystem = gameObject.AddComponent<ParticleSystem>();
        DestroyAfterTime destroyer = gameObject.AddComponent<DestroyAfterTime>();

        // set kill time
        destroyer.objectToDestroy = particleSystem;
        destroyer.destroySeconds = duration;

        // set particle
        particleSystem.startLifetime = duration;
        particleSystem.startSpeed = speed;
        particleSystem.startSize = size;
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.burstCount = 1;
        emission.SetBurst(0, new ParticleSystem.Burst(0, count));
        emission.rate = 0;
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.scale = directionScale;

        // set particle texture
        if (particleRenderer == null)
        {
            gameObject.AddComponent<ParticleSystemRenderer>();
            particleRenderer = gameObject.GetComponent<ParticleSystemRenderer>();
        }
        manager.assetPacketHandler.textureAssetManager.setTexture(manager, particleRenderer, texture);
    }

    public void useInstructions(string run, Instruction[] instructions)
    {
        switch(run)
        {
            case "now":
                Instruction.runInstructions(manager, instructions);
                break;
            case "on_update":
                onUpdateInstructions = instructions;
                break;
            case "on_move":
                onMoveInstructions = instructions;
                break;
            case "on_rotate":
                onRotateInstructions = instructions;
                break;
            case "on_scale":
                onScaleInstructions = instructions;
                break;
            default:
                Debug.LogError("Unknown entity run time " + run);
                break;
        }
    }
}
