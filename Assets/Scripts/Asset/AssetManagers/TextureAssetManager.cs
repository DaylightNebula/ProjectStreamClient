using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureAssetManager : AssetManager
{

    List<WaitingForTexture> waitingForTextureList = new List<WaitingForTexture>();
    List<WaitingForParticleTexture> waitingForParticleTextureList = new List<WaitingForParticleTexture>();
    List<int> requestedTextures = new List<int>();

    public override int getAssetID() => 2;
    public override int getPacketID() => 4;

    public override void Request(Manager manager, int id)
    {
        // make sure we are not already requesting a texture
        if (requestedTextures.Contains(id)) return;

        // request the texture from the asset server and update the requesting list
        manager.assetClient.sendPacket(0x03, BitConverter.GetBytes(id));
        requestedTextures.Add(id);
    }

    public override void ProcessData(Manager manager, byte[] data)
    {
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
                mat.SetTexture("_EmissionMap", texture);
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

    public void setTexture(Manager manager, Material mat, int textureID, int typeID)
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

    public void setTexture(Manager manager, ParticleSystemRenderer particle, int textureID)
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
        public int textureID;
        public int typeID;

        public bool shouldRemove = false;

        public WaitingForTexture(Material mat, int textureID, int typeID)
        {
            this.mat = mat;
            this.textureID = textureID;
            this.typeID = typeID;
        }
    }
    class WaitingForParticleTexture
    {
        public ParticleSystemRenderer particle;
        public int textureID;

        public bool shouldRemove = false;

        public WaitingForParticleTexture(ParticleSystemRenderer particle, int textureID)
        {
            this.particle = particle;
            this.textureID = textureID;
        }
    }
}
