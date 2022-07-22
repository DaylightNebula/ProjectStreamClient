using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MaterialAssetManager : AssetManager
{
    List<WaitingForMaterial> waitingForMaterialList = new List<WaitingForMaterial>();
    public List<string> requestedMaterials = new List<string>();

    public override int getAssetID() => 1;
    public override int getPacketID() => 2;

    int counter = 0;
    public override void ProcessData(Manager manager, byte[] data)
    {
        counter = 0;
        // unpack header
        string id = readStringFromByteArray(data);

        // unpack rest of packet
        string albedo_texture_id = readStringFromByteArray(data);
        string metallic_texture_id = readStringFromByteArray(data);
        float metallic_strength = data[counter] / 255f;
        float smoothness = data[counter + 1] / 255f;
        counter += 2;
        string normal_texture_id = readStringFromByteArray(data);
        string height_texture_id = readStringFromByteArray(data);
        string occlusion_texture_id = readStringFromByteArray(data);
        string detail_texture_id = readStringFromByteArray(data);
        string emission_texture_id = readStringFromByteArray(data);

        // create material
        Material mat = new Material(manager.shader);
        mat.name = "mat_" + id;

        // set smoothness and metallic strength
        mat.SetFloat("_Metallic", metallic_strength);
        mat.SetFloat("_Glossiness", smoothness);

        // set materials
        TextureAssetManager textureManager = manager.assetPacketHandler.textureAssetManager;
        textureManager.setTexture(manager, mat, albedo_texture_id, ((int)TextureType.ALBEDO));
        if (metallic_texture_id != "") textureManager.setTexture(manager, mat, metallic_texture_id, ((int)TextureType.METALLIC));
        if (normal_texture_id != "") textureManager.setTexture(manager, mat, normal_texture_id, ((int)TextureType.NORMAL_MAP));
        if (height_texture_id != "") textureManager.setTexture(manager, mat, height_texture_id, ((int)TextureType.HEIGHT_MAP));
        if (occlusion_texture_id != "") textureManager.setTexture(manager, mat, occlusion_texture_id, ((int)TextureType.OCCLUSION));
        if (detail_texture_id != "") textureManager.setTexture(manager, mat, detail_texture_id, ((int)TextureType.DETAIL_MAP));
        if (emission_texture_id != "") textureManager.setTexture(manager, mat, emission_texture_id, ((int)TextureType.EMISSION_MAP));

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

    private string readStringFromByteArray(byte[] bytes)
    {
        // get length, if it is 0, return a blank string
        int length = BitConverter.ToInt32(bytes, counter);
        if (length == 0)
        {
            counter += 4;
            return "";
        }

        // get string bytes from data
        byte[] string_bytes = new byte[length];
        Buffer.BlockCopy(bytes, counter + 4, string_bytes, 0, length);

        // update counter and return string bytes converted to a string
        counter += 4 + length;
        return Encoding.UTF8.GetString(string_bytes);
    }

    private bool shouldRemove(WaitingForMaterial waiting)
    {
        return waiting.shouldRemove;
    }

    public void setMaterial(Manager manager, EntityManager entityManager)
    {
        if (manager.materials.ContainsKey(entityManager.material))
        {
            entityManager.meshRenderer.material = manager.materials[entityManager.material];
        }
        else
        {
            lock (waitingForMaterialList)
            {
                waitingForMaterialList.Add(new WaitingForMaterial(entityManager, entityManager.material));
            }
            Request(manager, entityManager.material);
        }
    }

    public override void Request(Manager manager, string id)
    {
        // if we have the requested sound, return
        if (requestedMaterials.Contains(id)) return;

        // build request packet
        byte[] idBytes = BitConverter.GetBytes(id.Length);
        byte[] idStringBytes = Encoding.UTF8.GetBytes(id);
        byte[] packet = new byte[4 + id.Length];
        Buffer.BlockCopy(idBytes, 0, packet, 0, 4);
        Buffer.BlockCopy(idStringBytes, 0, packet, 4, idStringBytes.Length);

        // call request packet
        manager.assetClient.sendPacket(
            0x01,
            packet
        );

        // add id to requested sounds
        requestedMaterials.Add(id);
    }

    class WaitingForMaterial
    {
        public EntityManager entityManager;
        public string materialID;

        public bool shouldRemove = false;

        public WaitingForMaterial(EntityManager entityManager, string materialID)
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
