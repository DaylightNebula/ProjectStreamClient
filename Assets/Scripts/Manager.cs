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

    void Start()
    {
        // set last positions for tracked devices deltas
        UpdateLastPositionsOfTrackedDevices();
    }

    void FixedUpdate()
    {
        // update server connections
        if (behaviorClient != null) behaviorClient.update();
        if (assetClient != null) assetClient.update();

        // update instruction manager
        if (instructionManager != null) instructionManager.currentlyRunning();

        // update tracked positions of tracked devices
        SendPositionDataForTrackedDevice(0x02, hmd.transform.position, hmd.transform.rotation.eulerAngles, hmdLastPosition, hmdLastRotation);
        SendPositionDataForTrackedDevice(0x00, leftController.transform.position, leftController.transform.rotation.eulerAngles, lControllerLastPosition, lControllerLastRotation);
        SendPositionDataForTrackedDevice(0x01, rightController.transform.position, rightController.transform.rotation.eulerAngles, rControllerLastPosition, rControllerLastRotation);

        // update last positions for deltas
        UpdateLastPositionsOfTrackedDevices();
    }

    private void SendPositionDataForTrackedDevice(byte id, Vector3 currentPosition, Vector3 currentRotation, Vector3 lastPosition, Vector3 lastRotation)
    {
        // make sure behavior client exists
        if (behaviorClient == null) return;

        // get deltas
        Vector3 positionDelta = currentPosition - lastPosition;
        Vector3 rotationDelta = currentRotation - lastRotation;

        // build float array
        float[] floats = new float[12];
        floats[0] = currentPosition.x;
        floats[1] = currentPosition.y;
        floats[2] = currentPosition.z;
        floats[3] = currentRotation.x;
        floats[4] = currentRotation.y;
        floats[5] = currentRotation.z;
        floats[6] = positionDelta.x;
        floats[7] = positionDelta.y;
        floats[8] = positionDelta.z;
        floats[9] = rotationDelta.x;
        floats[10] = rotationDelta.y;
        floats[11] = rotationDelta.z;

        // build packet
        byte[] data = new byte[49];
        data[0] = id;
        Buffer.BlockCopy(floats, 0, data, 1, 48);

        // send packet
        behaviorClient.sendPacket(3, data);
    }

    private void UpdateLastPositionsOfTrackedDevices()
    {
        hmdLastPosition = hmd.transform.position;
        hmdLastRotation = hmd.transform.rotation.eulerAngles;
        lControllerLastPosition = leftController.transform.position;
        lControllerLastRotation = leftController.transform.rotation.eulerAngles;
        rControllerLastPosition = rightController.transform.position;
        rControllerLastRotation = rightController.transform.rotation.eulerAngles;
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
}
