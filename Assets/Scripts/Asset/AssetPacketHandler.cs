using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AssetPacketHandler
{

    Manager manager;
    AssetClient client;

    public Dictionary<int, AssetManager> assetManagers = new Dictionary<int, AssetManager>();
    public TextureAssetManager textureAssetManager = new TextureAssetManager();
    public MaterialAssetManager materialAssetManager = new MaterialAssetManager();
    public MeshAssetManager meshAssetManager = new MeshAssetManager();
    public SoundAssetManager soundAssetManager = new SoundAssetManager();

    public AssetPacketHandler(Manager manager)
    {
        this.manager = manager;
        assetManagers.Add(textureAssetManager.getAssetID(), textureAssetManager);
        assetManagers.Add(materialAssetManager.getAssetID(), materialAssetManager);
        assetManagers.Add(meshAssetManager.getAssetID(), meshAssetManager);
        assetManagers.Add(soundAssetManager.getAssetID(), soundAssetManager);
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
            case 7:
                Debug.LogWarning("We do not have the sound cache!");
                break;
            case 8:
                soundAssetManager.ProcessData(manager, data);
                break;
            default:
                break;
        }
    }

    public void setTexture(Material mat, int textureID, int typeID){ textureAssetManager.setTexture(manager, mat, textureID, typeID); }
    public void setMaterial(EntityManager entityManager) { materialAssetManager.setMaterial(manager, entityManager); }
    public void setMesh(EntityManager entityManager) { meshAssetManager.setMesh(manager, entityManager); }
}
