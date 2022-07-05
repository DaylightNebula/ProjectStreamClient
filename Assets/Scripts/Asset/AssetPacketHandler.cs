using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AssetPacketHandler
{

    Manager manager;
    AssetClient client;

    public TextureAssetManager textureAssetManager = new TextureAssetManager();
    public MaterialAssetManager materialAssetManager = new MaterialAssetManager();
    public MeshAssetManager meshAssetManager = new MeshAssetManager();

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
                materialAssetManager.ProcessData(manager, data);
                break;
            case 3:
                Debug.LogWarning("We do not have the texture cache!");
                break;
            case 4:
                textureAssetManager.ProcessData(manager, data);
                break;
            case 5:
                Debug.LogWarning("We do not have the mesh cache!");
                break;
            case 6:
                meshAssetManager.ProcessData(manager, data);
                break;
            default:
                break;
        }
    }

    public void setTexture(Material mat, int textureID, int typeID){ textureAssetManager.setTexture(manager, mat, textureID, typeID); }
    public void setMaterial(Renderer renderer, int materialID) { materialAssetManager.setMaterial(manager, renderer, materialID); }
    public void setMesh(MeshFilter filter, int meshID) { meshAssetManager.setMesh(manager, filter, meshID); }
}
