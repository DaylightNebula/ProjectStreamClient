using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshAssetManager : AssetManager
{
    List<WaitingForMesh> waitingForMesh = new List<WaitingForMesh>();
    public List<int> requestedMesh = new List<int>();

    public override int getAssetID() => 0;
    public override int getPacketID() => 6;

    public override void Request(Manager manager, int id)
    {
        if (requestedMesh.Contains(id)) return;
        manager.assetClient.sendPacket(0x05, BitConverter.GetBytes(id));
        requestedMesh.Add(id);
    }

    public override void ProcessData(Manager manager, byte[] data)
    {
        int counter = 8;

        // unpack header
        int id = BitConverter.ToInt32(data, 0);
        int numMeshInstances = BitConverter.ToInt32(data, 4);

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
                if (waiting.meshID == id)
                {
                    if (waiting.filter != null) waiting.filter.mesh = meshes[0]; // todo better handling for multiple instances
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

    public void setMesh(Manager manager, MeshFilter filter, int meshID)
    {
        if (manager.meshes.ContainsKey(meshID))
        {
            filter.mesh = manager.meshes[meshID];
        }
        else
        {
            lock (waitingForMesh)
            {
                waitingForMesh.Add(new WaitingForMesh(filter, meshID));
            }
            Request(manager, meshID);
        }
    }

    class WaitingForMesh
    {
        public MeshFilter filter;
        public int meshID;

        public bool shouldRemove = false;

        public WaitingForMesh(MeshFilter filter, int meshID)
        {
            this.filter = filter;
            this.meshID = meshID;
        }
    }
}
