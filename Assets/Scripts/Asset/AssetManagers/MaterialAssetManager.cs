using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialAssetManager : AssetManager
{
    List<WaitingForMaterial> waitingForMaterialList = new List<WaitingForMaterial>();
    public List<int> requestedMaterials = new List<int>();

    public override int getAssetID() => 1;
    public override int getPacketID() => 2;

    public override void ProcessData(Manager manager, byte[] data)
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
        TextureAssetManager textureManager = manager.assetPacketHandler.textureAssetManager;
        textureManager.setTexture(manager, mat, albedo_texture_id, ((int)TextureType.ALBEDO));
        if (metallic_texture_id != -1) textureManager.setTexture(manager, mat, metallic_texture_id, ((int)TextureType.METALLIC));
        if (normal_texture_id != -1) textureManager.setTexture(manager, mat, normal_texture_id, ((int)TextureType.NORMAL_MAP));
        if (height_texture_id != -1) textureManager.setTexture(manager, mat, height_texture_id, ((int)TextureType.HEIGHT_MAP));
        if (occlusion_texture_id != -1) textureManager.setTexture(manager, mat, occlusion_texture_id, ((int)TextureType.OCCLUSION));
        if (detail_texture_id != -1) textureManager.setTexture(manager, mat, detail_texture_id, ((int)TextureType.DETAIL_MAP));
        if (emission_texture_id != -1) textureManager.setTexture(manager, mat, emission_texture_id, ((int)TextureType.EMISSION_MAP));

        if (requestedMaterials.Contains(id)) requestedMaterials.Remove(id);

        // update waiting renderers
        lock (waitingForMaterialList)
        {
            foreach (WaitingForMaterial waiting in waitingForMaterialList)
            {
                if (id == waiting.materialID)
                {
                    if (waiting.entityManager.meshRenderer != null) waiting.entityManager.meshRenderer.material = mat;
                    waiting.shouldRemove = true;
                }
            }
            waitingForMaterialList.RemoveAll(shouldRemove);
        }
    }

    private bool shouldRemove(WaitingForMaterial waiting)
    {
        return waiting.shouldRemove;
    }

    public void setMaterial(Manager manager, EntityManager entityManager)
    {
        if (manager.materials.ContainsKey(entityManager.materialID))
        {
            entityManager.meshRenderer.material = manager.materials[entityManager.materialID];
        }
        else
        {
            lock (waitingForMaterialList)
            {
                waitingForMaterialList.Add(new WaitingForMaterial(entityManager, entityManager.materialID));
            }
            Request(manager, entityManager.materialID);
        }
    }

    public override void Request(Manager manager, int id)
    {
        if (requestedMaterials.Contains(id)) return;
        manager.assetClient.sendPacket(0x01, BitConverter.GetBytes(id));
        requestedMaterials.Add(id);
    }

    class WaitingForMaterial
    {
        public EntityManager entityManager;
        public int materialID;

        public bool shouldRemove = false;

        public WaitingForMaterial(EntityManager entityManager, int materialID)
        {
            this.entityManager = entityManager;
            this.materialID = materialID;
        }
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
}
