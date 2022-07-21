using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public string platform;
    public string behaviorAddress = "75.161.35.119";
    public int behaviorPort = 35524;

    public AssetClient assetClient;
    public AssetPacketHandler assetPacketHandler;

    public BehaviorClient behaviorClient;
    public BehaviorPacketHandler behaviorPacketHandler;

    public XMLDecoder xmlDecoder;

    public ActionManager actionManager;

    public EffectManager effectManager;

    [SerializeField]
    public PlatformPlayer[] players;
    public GameObject currentPlayer;

    public GameObject baseObject;
    public Shader shader;
    public Shader particleShader;
    public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    public Dictionary<string, Material> materials = new Dictionary<string, Material>();
    public Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
    public Dictionary<string, GameObject> entities = new Dictionary<string, GameObject>();
    private Dictionary<string, KeyValuePair<Vector3, Vector3>> points = new Dictionary<string, KeyValuePair<Vector3, Vector3>>();
    private Dictionary<string, float> variables = new Dictionary<string, float>();

    void Awake()
    {
        // create input manager
        actionManager = new ActionManager(this);

        // create behavior server connections
        behaviorPacketHandler = new BehaviorPacketHandler(this);
        behaviorClient = new BehaviorClient(behaviorPacketHandler);
        behaviorPacketHandler.setClient(behaviorClient);
        behaviorClient.start(behaviorAddress, behaviorPort);

        // create xml decoder
        xmlDecoder = new XMLDecoder(this);

        // set active state of platforms
        foreach (PlatformPlayer platformPlayer in players)
        {
            bool isPlatform = platformPlayer.platform == platform;
            if (isPlatform)
            {
                currentPlayer = GameObject.Instantiate(platformPlayer.player, new Vector3(0f, 0f, 0f), Quaternion.identity);
                break;
            }
        }
    }

    public float getVariable(string name)
    {
        if (variables.ContainsKey(name))
            return variables[name];
        else
            return 0f;
    }

    public void setVariable(string name, float value)
    {
        variables[name] = value;
    }

    public void makeMeshExist(string mesh)
    {
        if (!meshes.ContainsKey(mesh) && !assetPacketHandler.meshAssetManager.requestedMesh.Contains(mesh))
            assetPacketHandler.meshAssetManager.Request(this, mesh);
    }

    public void makeMaterialExist(string material)
    {
        if (!meshes.ContainsKey(material) && !assetPacketHandler.materialAssetManager.requestedMaterials.Contains(material))
            assetPacketHandler.materialAssetManager.Request(this, material);
    }

    public void setMaterial(EntityManager entityManager)
    {
        assetPacketHandler.setMaterial(entityManager);
    }

    public void setMesh(EntityManager entityManager)
    {
        assetPacketHandler.setMesh(entityManager);
    }

    public void ConnectToAssetServer(string address, int port)
    {
        // create asset server connection
        assetPacketHandler = new AssetPacketHandler(this);
        assetClient = new AssetClient(assetPacketHandler);
        assetPacketHandler.setClient(assetClient);
        assetClient.start(address, port);
    }

    void Update()
    {
        // update actions
        if (assetClient != null)
            actionManager.update();
    }

    void FixedUpdate()
    {
        // update server connections
        if (behaviorClient != null) behaviorClient.update();
        if (assetClient != null) assetClient.update();
    }

    public void UpdatePointLocation(string point, Vector3 position, Vector3 rotation)
    {
        points[point] = new KeyValuePair<Vector3, Vector3>(position, rotation);
        //SendPointUpdateLocation(point, position, rotation);
    }

    public KeyValuePair<Vector3, Vector3> GetPointLocation(string point)
    {
        return points[point];
    }

    public bool DoesPointExist(string point)
    {
        return points.ContainsKey(point);
    }

    public void SendButtonPress(byte buttonID)
    {
        if (behaviorClient == null) return;

        // build packet
        byte[] data = new byte[1];
        data[0] = buttonID;

        // send packet
        behaviorClient.sendPacket(4, data);
    }

    public void SendButtonRelease(byte buttonID)
    {
        if (behaviorClient == null) return;

        // build packet
        byte[] data = new byte[1];
        data[0] = buttonID;

        // send packet
        behaviorClient.sendPacket(5, data);
    }

    void OnApplicationQuit()
    {
        // dispose server connections
        if (behaviorClient != null) behaviorClient.dispose();
        if (assetClient != null) assetClient.dispose();
    }

    public void DestroyUnityObject(UnityEngine.Object obj)
    {
        Destroy(obj);
    }


    [Serializable]
    public struct PlatformPlayer { public string platform; public GameObject player; }
}
