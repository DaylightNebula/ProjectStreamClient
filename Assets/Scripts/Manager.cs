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

    public InstructionManager instructionManager;
    public InputManager inputManager;

    public GameObject leftController;
    public GameObject rightController;

    public GameObject baseObject;
    public Shader shader;
    public Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
    public Dictionary<int, Material> materials = new Dictionary<int, Material>();
    public Dictionary<int, Mesh> meshes = new Dictionary<int, Mesh>();
    public Dictionary<int, GameObject> entities = new Dictionary<int, GameObject>();
    public Dictionary<int, KeyValuePair<Vector3, Vector3>> points = new Dictionary<int, KeyValuePair<Vector3, Vector3>>();

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
        instructionManager = new InstructionManager(this);

        // create input manager
        inputManager = new InputManager(this, instructionManager);

        // create behavior server connections
        behaviorPacketHandler = new BehaviorPacketHandler(this);
        behaviorClient = new BehaviorClient(behaviorPacketHandler);
        behaviorPacketHandler.setClient(behaviorClient);
        behaviorClient.start(behaviorAddress, behaviorPort);
    }

    public void makeMeshExist(int meshID)
    {
        if (!meshes.ContainsKey(meshID) && !assetPacketHandler.requestedMesh.Contains(meshID))
            assetPacketHandler.requestMesh(meshID);
    }

    public void makeMaterialExist(int materialID)
    {
        if (!meshes.ContainsKey(materialID) && !assetPacketHandler.requestedMaterials.Contains(materialID))
            assetPacketHandler.requestMaterial(materialID);
    }

    public void setMaterial(Renderer renderer, int materialID)
    {
        assetPacketHandler.setMaterial(renderer, materialID);
    }

    public void setMesh(MeshFilter filter, int meshID)
    {
        assetPacketHandler.setMesh(filter, meshID);
    }

    public void ConnectToAssetServer(string address, int port)
    {
        // create asset server connection
        assetPacketHandler = new AssetPacketHandler(this);
        assetClient = new AssetClient(assetPacketHandler);
        assetPacketHandler.setClient(assetClient);
        assetClient.start(address, port);

        // tell instruction manager the asset server is connected
        instructionManager.assetServerConnected();
    }

    void FixedUpdate()
    {
        // update server connections
        if (behaviorClient != null) behaviorClient.update();
        if (assetClient != null) assetClient.update();

        // update instruction manager
        if (instructionManager != null) instructionManager.currentlyRunning();
    }

    void OnApplicationQuit()
    {
        // dispose server connections
        if (behaviorClient != null) behaviorClient.dispose();
        if (assetClient != null) assetClient.dispose();
    }
}
