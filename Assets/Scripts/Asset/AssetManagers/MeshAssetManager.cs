using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MeshAssetManager : AssetManager
{
    public List<WaitingForMesh> waitingForMesh = new List<WaitingForMesh>();
    public List<string> requestedMesh = new List<string>();

    public override int getAssetID() => 0;
    public override int getPacketID() => 6;

    public override void Request(Manager manager, string id)
    {
        // if we have the requested sound, return
        if (requestedMesh.Contains(id)) return;

        // build request packet
        byte[] idBytes = BitConverter.GetBytes(id.Length);
        byte[] idStringBytes = Encoding.UTF8.GetBytes(id);
        byte[] packet = new byte[4 + id.Length];
        Buffer.BlockCopy(idBytes, 0, packet, 0, 4);
        Buffer.BlockCopy(idStringBytes, 0, packet, 4, idStringBytes.Length);

        // call request packet
        manager.assetClient.sendPacket(
            0x05,
            packet
        );

        // add id to requested sounds
        requestedMesh.Add(id);
    }

    public override void ProcessData(Manager manager, byte[] data)
    {
        // unpack header
        int id_length = BitConverter.ToInt32(data, 0);
        byte[] id_bytes = new byte[id_length];
        Buffer.BlockCopy(data, 4, id_bytes, 0, id_length);
        string id = Encoding.UTF8.GetString(id_bytes);
        int numMeshInstances = BitConverter.ToInt32(data, id_length + 4);
        int counter = 8 + id_length;

        // create mesh instances list
        Mesh[] meshes = new Mesh[numMeshInstances];

        // loop through each mesh instances
        for (int meshIndex = 0; meshIndex < numMeshInstances; meshIndex++)
        {
            // get vertices
            int numVertices = BitConverter.ToInt32(data, counter);
            counter += 4;
            Vector3[] vertices = new Vector3[numVertices];
            for (int vIdx = 0; vIdx < numVertices; vIdx++)
            {
                Vector3 vertex = new Vector3(
                    BitConverter.ToSingle(data, counter),
                    BitConverter.ToSingle(data, counter + 4),
                    BitConverter.ToSingle(data, counter + 8)
                );
                counter += 12;
                vertices[vIdx] = vertex;
            }

            // get normals
            int numNormals = BitConverter.ToInt32(data, counter);
            counter += 4;
            Vector3[] normals = new Vector3[numNormals];
            for (int nIdx = 0; nIdx < numNormals; nIdx++)
            {
                Vector3 normal = new Vector3(
                    BitConverter.ToSingle(data, counter),
                    BitConverter.ToSingle(data, counter + 4),
                    BitConverter.ToSingle(data, counter + 8)
                );
                counter += 12;
                normals[nIdx] = normal;
            }

            // get tex coords
            int numTexCoords = BitConverter.ToInt32(data, counter);
            counter += 4;
            Vector2[] texCoords = new Vector2[numTexCoords];
            for (int tIdx = 0; tIdx < numTexCoords; tIdx++)
            {
                Vector2 texCoord = new Vector2(
                    BitConverter.ToSingle(data, counter),
                    BitConverter.ToSingle(data, counter + 4)
                );
                counter += 8;
                texCoords[tIdx] = texCoord;
            }

            // get indices
            int numIndices = BitConverter.ToInt32(data, counter);
            counter += 4;
            int[] indices = new int[numIndices];
            for (int iIdx = 0; iIdx < numIndices; iIdx++)
            {
                indices[iIdx] = BitConverter.ToInt32(data, counter);
                counter += 4;
            }

            // create mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = texCoords;
            mesh.triangles = indices;
            meshes[meshIndex] = mesh;
        }

        // update waiting mesh filters
        lock (waitingForMesh)
        {
            foreach (WaitingForMesh waiting in waitingForMesh)
            {
                if (waiting.mesh == id)
                {
                    if (waiting.entityManager.meshFilter != null) waiting.entityManager.meshFilter.mesh = meshes[0]; // todo better handling for multiple instances
                    if (waiting.entityManager.collider != null) waiting.entityManager.collider.sharedMesh = meshes[0];
                    waiting.shouldRemove = true;
                }
            }
            waitingForMesh.RemoveAll(shouldRemove);
        }

        if (requestedMesh.Contains(id)) requestedMesh.Remove(id);
    }

    private bool shouldRemove(WaitingForMesh waiting)
    {
        return waiting.shouldRemove;
    }

    public void setMesh(Manager manager, EntityManager entityManager)
    {
        if (manager.meshes.ContainsKey(entityManager.mesh))
        {
            entityManager.meshFilter.mesh = manager.meshes[entityManager.mesh];
        }
        else
        {
            lock (waitingForMesh)
            {
                waitingForMesh.Add(new WaitingForMesh(entityManager, entityManager.mesh));
            }
            Request(manager, entityManager.mesh);
        }
    }

    public class WaitingForMesh
    {
        public EntityManager entityManager;
        public string mesh;

        public bool shouldRemove = false;

        public WaitingForMesh(EntityManager entityManager, string mesh)
        {
            this.entityManager = entityManager;
            this.mesh = mesh;
        }
    }
}
