using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AssetPacketHandler
{

    Manager manager;
    AssetClient client;

    List<WaitingForTexture> waitingForTextureList = new List<WaitingForTexture>();
    List<WaitingForMaterial> waitingForMaterialList = new List<WaitingForMaterial>();
    List<WaitingForMesh> waitingForMesh = new List<WaitingForMesh>();

    public List<int> requestedMesh = new List<int>();
    public List<int> requestedMaterials = new List<int>();

    public AssetPacketHandler(Manager manager)
    {
        this.manager = manager;
    }

    public void setClient(AssetClient client)
    {
        this.client = client;
    }

    public void processPacket(int packetID, byte[] data)
    {
        switch (packetID)
        {
            case 0:
                Debug.Log("Hello packet received");
                break;
            case 1:
                Debug.LogWarning("We do not have the material cache!");
                break;
            case 2:
                processMaterial(data);
                break;
            case 3:
                Debug.LogWarning("We do not have the texture cache!");
                break;
            case 4:
                processTexture(data);
                break;
            case 5:
                Debug.LogWarning("We do not have the mesh cache!");
                break;
            case 6:
                processMesh(data);
                break;
            default:
                break;
        }
    }
    
    public async void processTexture(byte[] data)
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log("Loading texture of size " + data.Length + " with start time " + startTime);
        // unpack packet header
        int id = BitConverter.ToInt32(data, 0);
        Array.Reverse(data);
        Array.Resize(ref data, data.Length - 4);
        Array.Reverse(data);

        // get texture
        Texture2D texture = null;
        if (manager.textures.ContainsKey(id))
        {
            texture = manager.textures[id];
        }
        else
        {
            texture = new Texture2D(1, 1);
            manager.textures.Add(id, texture);
        }

        // load texture
        texture.LoadImage(data);

        // update waiting materials
        Debug.Log("Applying " + waitingForTextureList.Count + " textures!");
        lock (waitingForTextureList)
        {
            foreach (WaitingForTexture waiting in waitingForTextureList)
            {
                if (id == waiting.textureID)
                {
                    applyTexture(waiting.mat, texture, waiting.typeID);
                    //waitingForTextureList.Remove(waiting);
                }
            }
        }

        Debug.Log("Total time = " + (Time.realtimeSinceStartup - startTime));
    }

    private void asyncLoadTexture(Texture2D texture, int id, byte[] data)
    {
        Debug.Log("Starting async load");
        // load texture
        texture.LoadImage(data);

        // update waiting materials
        Debug.Log("Applying " + waitingForTextureList.Count + " textures!");
        lock (waitingForTextureList)
        {
            foreach (WaitingForTexture waiting in waitingForTextureList)
            {
                if (id == waiting.textureID)
                {
                    applyTexture(waiting.mat, texture, waiting.typeID);
                    //waitingForTextureList.Remove(waiting);
                }
            }
        }
    }

    public void processMaterial(byte[] data)
    {
        // unpack packet
        int id = BitConverter.ToInt32(data, 0);
        int albedo_texture_id = BitConverter.ToInt32(data, 4);
        int metallic_texture_id = BitConverter.ToInt32(data, 8);
        float metallic_strength = data[12] / 255f;
        float smoothness = data[13] / 255f;
        int normal_texture_id = BitConverter.ToInt32(data, 14);
        int height_texture_id = BitConverter.ToInt32(data, 18);
        int occlusion_texture_id = BitConverter.ToInt32(data, 22);
        int detail_texture_id = BitConverter.ToInt32(data, 26);
        int emission_texture_id = BitConverter.ToInt32(data, 30);

        // create material
        Material mat = new Material(manager.shader);
        mat.name = "mat_" + id;

        // set smoothness and metallic strength
        mat.SetFloat("_Metallic", metallic_strength);
        mat.SetFloat("_Glossiness", smoothness);

        // set materials
        setTexture(mat, albedo_texture_id, ((int)TextureType.ALBEDO));
        if (metallic_texture_id != -1) setTexture(mat, metallic_texture_id, ((int)TextureType.METALLIC));
        if (normal_texture_id != -1) setTexture(mat, normal_texture_id, ((int)TextureType.NORMAL_MAP));
        if (height_texture_id != -1) setTexture(mat, height_texture_id, ((int)TextureType.HEIGHT_MAP));
        if (occlusion_texture_id != -1) setTexture(mat, occlusion_texture_id, ((int)TextureType.OCCLUSION));
        if (detail_texture_id != -1) setTexture(mat, detail_texture_id, ((int)TextureType.DETAIL_MAP));
        if (emission_texture_id != -1) setTexture(mat, emission_texture_id, ((int)TextureType.EMISSION_MAP));

        // update waiting renderers
        lock(waitingForMaterialList)
        {
            foreach (WaitingForMaterial waiting in waitingForMaterialList)
            {
                if (id == waiting.materialID)
                {
                    if (waiting.renderer != null) waiting.renderer.material = mat;
                }
            }
        }
    }

    public void setTexture(Material mat, int textureID, int typeID)
    {
        if (manager.textures.ContainsKey(textureID))
        {
            applyTexture(mat, manager.textures[textureID], typeID);
        }
        else
        {
            lock (waitingForTextureList)
            {
                waitingForTextureList.Add(new WaitingForTexture(mat, textureID, typeID));
            }
            requestTexture(textureID);
        }
    }

    public void setMaterial(Renderer renderer, int materialID)
    {
        if (manager.materials.ContainsKey(materialID))
        {
            renderer.material = manager.materials[materialID];
        }
        else
        {
            lock (waitingForMaterialList)
            {
                waitingForMaterialList.Add(new WaitingForMaterial(renderer, materialID));
            }
            requestMaterial(materialID);
        }
    }

    public void setMesh(MeshFilter filter, int meshID)
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
            requestMesh(meshID);
        }
    }

    private void applyTexture(Material mat, Texture texture, int typeID)
    {
        // for each type, apply the texture appropriatly
        switch (typeID)
        {
            case 0:
                mat.SetTexture("_MainTex", texture);
                break;
            case 1:
                mat.SetTexture("_MetallicGlossMap", texture);
                break;
            case 2:
                break;
            case 3:
                mat.SetTexture("_BumpMap", texture);
                break;
            case 4:
                mat.SetTexture("_ParallaxMap", texture);
                break;
            case 5:
                mat.SetTexture("_OcclusionMap", texture);
                break;
            case 6:
                mat.SetTexture("_DetailMask", texture);
                break;
            case 7:
                mat.SetTexture("_EmissionMap", texture);
                break;
            default:
                Debug.LogError("Could not apply texture with typeID " + typeID);
                break;
        }
    }

    public void processMesh(byte[] data)
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
        lock(waitingForMesh)
        {
            foreach (WaitingForMesh waiting in waitingForMesh)
            {
                if (waiting.meshID == id)
                {
                    if (waiting.filter != null) waiting.filter.mesh = meshes[0]; // todo better handling for multiple instances
                }
            }
        }
    }

    private void requestTexture(int textureID)
    {
        client.sendPacket(0x03, BitConverter.GetBytes(textureID));
    }

    public void requestMaterial(int id)
    {
        client.sendPacket(0x01, BitConverter.GetBytes(id));
        requestedMaterials.Add(id);
    }

    public void requestMesh(int id)
    {
        client.sendPacket(0x05, BitConverter.GetBytes(id));
        requestedMesh.Add(id);
    }

    enum TextureType
    {
        ALBEDO = 0,
        METALLIC = 1,
        SMOOTHNESS = 2,
        NORMAL_MAP = 3,
        HEIGHT_MAP = 4,
        OCCLUSION = 5,
        DETAIL_MAP = 6,
        EMISSION_MAP = 7
    }

    class WaitingForTexture 
    {
        public Material mat;
        public int textureID;
        public int typeID;

        public WaitingForTexture(Material mat, int textureID, int typeID)
        {
            this.mat = mat;
            this.textureID = textureID;
            this.typeID = typeID;
        }
    }

    class WaitingForMaterial
    {
        public Renderer renderer;
        public int materialID;

        public WaitingForMaterial(Renderer renderer, int materialID)
        {
            this.renderer = renderer;
            this.materialID = materialID;
        }
    }

    class WaitingForMesh
    {
        public MeshFilter filter;
        public int meshID;

        public WaitingForMesh(MeshFilter filter, int meshID)
        {
            this.filter = filter;
            this.meshID = meshID;
        }
    }
}
