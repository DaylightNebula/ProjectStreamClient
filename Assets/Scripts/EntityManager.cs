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

    Vector3 position;
    Quaternion rotation;
    Vector3 scale;
    bool isActive = true;

    void Update()
    {
        // check if update to state
        if (position != transform.position || rotation != transform.rotation || scale != transform.localScale)
        {
            sendEntityTransformUpdate();
        }
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
    {
        /*// update tracking variables
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
        isActive = gameObject.activeSelf;

        // build int array for id
        int[] idArray = new int[1];
        idArray[0] = entityID;

        // build float array for positions
        Vector3 rotationEuler = rotation.eulerAngles;
        float[] floatArray = new float[9];
        floatArray[0] = position.x;
        floatArray[1] = position.y;
        floatArray[2] = position.z;
        floatArray[3] = rotationEuler.x;
        floatArray[4] = rotationEuler.y;
        floatArray[5] = rotationEuler.z;
        floatArray[6] = scale.x;
        floatArray[7] = scale.y;
        floatArray[8] = scale.z;

        // build packet
        byte[] data = new byte[41];
        Buffer.BlockCopy(idArray, 0, data, 0, 4);
        Buffer.BlockCopy(floatArray, 0, data, 4, 36);
        if (isActive) data[40] = 1; else data[40] = 0;

        // send update packet to client
        manager.behaviorClient.sendPacket(0x08, data);*/
    }

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
            ParticleSystemRenderer renderer = gameObject.GetComponent<ParticleSystemRenderer>();

            // if they exist, destroy them
            if (system != null) Destroy(system);
            if (renderer != null) Destroy(renderer);

            // cancel function
            return;
        }

        // create object and get components
        ParticleSystem particleSystem = gameObject.AddComponent<ParticleSystem>();
        DestroyAfterTime destroyer = gameObject.AddComponent<DestroyAfterTime>();

        // set kill time
        destroyer.destroySeconds = lifetime;

        // set particle
        particleSystem.startLifetime = duration;
        particleSystem.startSpeed = speed;
        particleSystem.startSize = size;
        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rate = rate;
        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.scale = directionScale;

        // set particle texture
        manager.assetPacketHandler.textureAssetManager.setTexture(manager, gameObject.AddComponent<ParticleSystemRenderer>(), texture);
    }
}
