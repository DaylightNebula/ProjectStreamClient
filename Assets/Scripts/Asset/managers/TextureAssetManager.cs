using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureAssetManager : AssetManager
{

    List<WaitingForTexture> waitingForTextureList = new List<WaitingForTexture>();
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

        // update requesting list
        if (requestedTextures.Contains(id)) requestedTextures.Remove(id);
    }

    private bool shouldRemove(WaitingForTexture waiting)
    {
        return waiting.shouldRemove;
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
}
