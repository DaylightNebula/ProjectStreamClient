using System;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public Manager manager;

    public int entityID;
    public int meshID = -1;
    public int materialID = -1;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider collider;
    public Rigidbody rigidbody;

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
        // update tracking variables
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
        manager.behaviorClient.sendPacket(0x08, data);
    }

    public void addMesh(int id)
    {
        // update ID and make sure everything needed for display exists
        meshID = id;
        addMeshFilterAndRenderer();

        // apply mesh
        manager.setMesh(this);
    }

    public void addMaterial(int id)
    {
        // update ID and make sure everything needed for display exists
        materialID = id;
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
        if (manager.meshes.ContainsKey(meshID))
            collider.sharedMesh = manager.meshes[meshID];
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
}
