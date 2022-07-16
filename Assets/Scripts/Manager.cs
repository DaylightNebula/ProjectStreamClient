using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class Manager : MonoBehaviour
{
    public string behaviorAddress = "localhost";
    public int behaviorPort = 35524;

    public AssetClient assetClient;
    public AssetPacketHandler assetPacketHandler;

    public BehaviorClient behaviorClient;
    public BehaviorPacketHandler behaviorPacketHandler;

    public InputManager inputManager;

    public GameObject hmd;
    public GameObject leftController;
    public GameObject rightController;
    Vector3 hmdLastPosition;
    Vector3 hmdLastRotation;
    Vector3 lControllerLastPosition;
    Vector3 lControllerLastRotation;
    Vector3 rControllerLastPosition;
    Vector3 rControllerLastRotation;

    public GameObject baseObject;
    public GameObject baseParticle;
    public Shader shader;
    public Shader particleShader;
    public Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
    public Dictionary<string, Material> materials = new Dictionary<string, Material>();
    public Dictionary<string, Mesh> meshes = new Dictionary<string, Mesh>();
    public Dictionary<string, GameObject> entities = new Dictionary<string, GameObject>();
    private Dictionary<string, KeyValuePair<Vector3, Vector3>> points = new Dictionary<string, KeyValuePair<Vector3, Vector3>>();

    public bool usingHeadset = true;
    DesktopMouseLook mouseLook;

    public InputAction aButton;
    public InputAction bButton;
    public InputAction xButton;
    public InputAction yButton;
    public InputAction lTriggerPress;
    public InputAction rTriggerPress;
    public InputAction lGripPress;
    public InputAction rGripPress;
    public InputAction lJoyPress;
    public InputAction rJoyPress;

    void Awake()
    {
        // create instruction manager
        //instructionManager = new InstructionManager(this);

        // create input manager
        inputManager = new InputManager(this);

        // create behavior server connections
        behaviorPacketHandler = new BehaviorPacketHandler(this);
        behaviorClient = new BehaviorClient(behaviorPacketHandler);
        behaviorPacketHandler.setClient(behaviorClient);
        behaviorClient.start(behaviorAddress, behaviorPort);
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

    void Start()
    {
        // if not using headset, initialize mouse look
        if (!usingHeadset)
        {
            mouseLook = new DesktopMouseLook();
            mouseLook.Start(hmd, leftController, rightController);
        }

        // enable vsync
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;
    }

    void Update()
    {
        // if not using headset, update mouse look
        if (!usingHeadset)
        {
            mouseLook.Update();
        }
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

    /*private void SendPointUpdateLocation(string point, Vector3 position, Vector3 rotation)
    {
        // make sure behavior client exists
        if (behaviorClient == null) return;

        // build int array for id
        int[] id = new int[1];
        id[0] = pointID;

        // build float array
        float[] floats = new float[12];
        floats[0] = position.x;
        floats[1] = position.y;
        floats[2] = position.z;
        floats[3] = rotation.x;
        floats[4] = rotation.y;
        floats[5] = rotation.z;

        // build packet
        byte[] data = new byte[28];
        Buffer.BlockCopy(id, 0, data, 0, 4);
        Buffer.BlockCopy(floats, 0, data, 4, 24);

        // send packet
        behaviorClient.sendPacket(7, data);
    }*/

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
}
