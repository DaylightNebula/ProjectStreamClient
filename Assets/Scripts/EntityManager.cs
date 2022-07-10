using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public Manager manager;

    public int meshID = -1;
    public int materialID = -1;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider collider;

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
}
