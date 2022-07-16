using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TextureAssetManager : AssetManager
{

    List<WaitingForTexture> waitingForTextureList = new List<WaitingForTexture>();
    List<WaitingForParticleTexture> waitingForParticleTextureList = new List<WaitingForParticleTexture>();
    List<string> requestedTextures = new List<string>();

    public override int getAssetID() => 2;
    public override int getPacketID() => 4;

    public override void Request(Manager manager, string id)
    {
        // if we have the requested sound, return
        if (requestedTextures.Contains(id)) return;

        // build request packet
        byte[] idBytes = BitConverter.GetBytes(id.Length);
        byte[] idStringBytes = Encoding.UTF8.GetBytes(id);
        byte[] packet = new byte[4 + id.Length];
        Buffer.BlockCopy(idBytes, 0, packet, 0, 4);
        Buffer.BlockCopy(idStringBytes, 0, packet, 4, idStringBytes.Length);

        // call request packet
        manager.assetClient.sendPacket(
            0x03,
            packet
        );

        // add id to requested sounds
        requestedTextures.Add(id);
    }

    public override void ProcessData(Manager manager, byte[] data)
    {
        // unpack packet header
        int id_length = BitConverter.ToInt32(data, 0);
        byte[] id_bytes = new byte[id_length];
        Buffer.BlockCopy(data, 4, id_bytes, 0, id_length);
        string id = Encoding.UTF8.GetString(id_bytes);

        // remove starting data
        Array.Reverse(data);
        Array.Resize(ref data, data.Length - (id_length + 4));
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
        lock (waitingForTextureList)
        {
            foreach (WaitingForTexture waiting in waitingForTextureList)
            {
                if (id == waiting.textureID)
                {
                    applyTexture(waiting.mat, texture, waiting.typeID);
                    waiting.shouldRemove = true;
                }
            }
            waitingForTextureList.RemoveAll(shouldRemove);
        }
        lock (waitingForParticleTextureList)
        {
            foreach (WaitingForParticleTexture waiting in waitingForParticleTextureList)
            {
                if (id == waiting.textureID)
                {
                    applyTexture(manager, waiting.particle, texture);
                    waiting.shouldRemove = true;
                }
            }
            waitingForTextureList.RemoveAll(shouldRemove);
        }

        // update requesting list
        if (requestedTextures.Contains(id)) requestedTextures.Remove(id);
    }

    private bool shouldRemove(WaitingForTexture waiting)
    {
        return waiting.shouldRemove;
    }

    private void applyTexture(Material mat, Texture2D texture, int typeID)
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
                mat.EnableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", texture);
                mat.SetColor("_EmissionColor", Color.white);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                break;
            default:
                Debug.LogError("Could not apply texture with typeID " + typeID);
                break;
        }
    }

    private void applyTexture(Manager manager, ParticleSystemRenderer particle, Texture2D texture)
    {
        Debug.Log("Updated particle textures for");
        Material material = new Material(manager.particleShader);
        material.SetTexture("_MainTex", texture);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        particle.material = material;
    }

    public void setTexture(Manager manager, Material mat, string textureID, int typeID)
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
            Request(manager, textureID);
        }
    }

    public void setTexture(Manager manager, ParticleSystemRenderer particle, string textureID)
    {
        if (manager.textures.ContainsKey(textureID))
        {
            applyTexture(manager, particle, manager.textures[textureID]);
        }
        else
        {
            lock (waitingForParticleTextureList)
            {
                waitingForParticleTextureList.Add(new WaitingForParticleTexture(particle, textureID));
            }
            Request(manager, textureID);
        }
    }

    class WaitingForTexture
    {
        public Material mat;
        public string textureID;
        public int typeID;

        public bool shouldRemove = false;

        public WaitingForTexture(Material mat, string textureID, int typeID)
        {
            this.mat = mat;
            this.textureID = textureID;
            this.typeID = typeID;
        }
    }
    class WaitingForParticleTexture
    {
        public ParticleSystemRenderer particle;
        public string textureID;

        public bool shouldRemove = false;

        public WaitingForParticleTexture(ParticleSystemRenderer particle, string textureID)
        {
            this.particle = particle;
            this.textureID = textureID;
        }
    }
}
