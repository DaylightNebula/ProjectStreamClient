using System;
using System.Collections.Generic;
using UnityEngine;

public class InstructionManager
{
    Manager manager;

    // instruction lists
    Instruction[] assetServerConnectInstructions;
    Instruction[] whenRunningInstructions;
    Instruction[][] onJoystickChangeInstructions = new Instruction[2][];
    Instruction[][] whenJoystickNotZeroInstructions = new Instruction[2][];
    Instruction[][] onTriggerChangeInstructions = new Instruction[2][];
    Instruction[][] whenTriggerNotZeroInstructions = new Instruction[2][];
    Instruction[][] onGripChangeInstructions = new Instruction[2][];
    Instruction[][] whenGripNotZeroInstructions = new Instruction[2][];
    Instruction[][] onButtonPress = new Instruction[11][];

    public InstructionManager(Manager manager)
    {
        this.manager = manager;
    }

    // instruction call functions
    public void assetServerConnected() { ExecuteInstructions(assetServerConnectInstructions);}
    public void currentlyRunning() { ExecuteInstructions(whenRunningInstructions); }
    public void joystickChanged(InstructionController con) { ExecuteInstructions(onJoystickChangeInstructions[(int) con]); }
    public void jotstickNotZero(InstructionController con) { ExecuteInstructions(whenJoystickNotZeroInstructions[(int)con]); }
    public void triggerChanged(InstructionController con) { ExecuteInstructions(onTriggerChangeInstructions[(int)con]); }
    public void triggerNotZero(InstructionController con) { ExecuteInstructions(whenTriggerNotZeroInstructions[(int)con]); }
    public void gripChanged(InstructionController con) { ExecuteInstructions(onGripChangeInstructions[(int)con]); }
    public void gripNotZero(InstructionController con) { ExecuteInstructions(whenGripNotZeroInstructions[(int)con]); }
    public void buttonPressed(InstructionButton button) { Debug.Log("Pressed " + ((int)button)); ExecuteInstructions(onButtonPress[(int)button]); }

    // function to apply instructions lists
    public void applyInstructionList(int execute_id, int execute_data, Instruction[] instructions)
    {
        switch (execute_id)
        {
            case 0:
                ExecuteInstructions(instructions);
                break;
            case 1:
                assetServerConnectInstructions = instructions;
                break;
            case 2:
                whenRunningInstructions = instructions;
                break;
            case 3:
                onJoystickChangeInstructions[execute_data] = instructions;
                break;
            case 4:
                whenJoystickNotZeroInstructions[execute_data] = instructions;
                break;
            case 5:
                onTriggerChangeInstructions[execute_data] = instructions;
                break;
            case 6:
                whenTriggerNotZeroInstructions[execute_data] = instructions;
                break;
            case 7:
                onGripChangeInstructions[execute_data] = instructions;
                break;
            case 8:
                whenGripNotZeroInstructions[execute_data] = instructions;
                break;
            case 9:
                onButtonPress[execute_data] = instructions;
                break;
            default:
                Debug.Log("No when execute function made for id " + execute_id);
                break;
        }
    }

    // functions to execute instructions
    public void ExecuteInstructions(Instruction[] instructions)
    {
        if (instructions == null) return;
        foreach (Instruction instruction in instructions)
        {
            ExecuteInstruction(instruction);
        }
    }

    public void ExecuteInstruction(Instruction instruction)
    {
        // unpack instruction
        int id = instruction.instructionID;
        byte[] data = instruction.instructionData;

        switch(id)
        {
            case 0: // spawn entity
                // unpack data
                int entity_id = BitConverter.ToInt32(data, 0);
                int mesh_id = BitConverter.ToInt32(data, 4);
                int material_id = BitConverter.ToInt32(data, 8);
                Vector3 position = new Vector3(BitConverter.ToSingle(data, 12), BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20));
                Quaternion rotation = Quaternion.Euler(BitConverter.ToSingle(data, 24), BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32));
                Vector3 scale = new Vector3(BitConverter.ToSingle(data, 36), BitConverter.ToSingle(data, 40), BitConverter.ToSingle(data, 44));

                // create entity
                GameObject entity = GameObject.Instantiate(manager.baseObject, position, rotation);
                manager.entities.Add(entity_id, entity);

                // apply mesh and material
                MeshFilter filter = entity.GetComponent<MeshFilter>();
                Renderer renderer = entity.GetComponent<Renderer>();
                manager.setMesh(filter, mesh_id);
                manager.setMaterial(renderer, material_id);

                // apply scale
                entity.transform.localScale = scale;

                // end
                break;
            case 1: // remove entity
                // unpack data
                entity_id = BitConverter.ToInt32(data, 0);

                // get entity
                entity = manager.entities[entity_id];

                // if does not exist, skip
                if (entity == null)
                {
                    Debug.LogError("Entity with id " + entity_id + " does not exist!");
                    break;
                }

                // destroy entity
                GameObject.Destroy(entity, 0f);

                // remove entity
                manager.entities.Remove(entity_id);

                // end
                break;
            case 2: // update entity transform
                // unpack data
                entity_id = BitConverter.ToInt32(data, 0);
                position = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
                rotation = Quaternion.Euler(BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24));
                scale = new Vector3(BitConverter.ToSingle(data, 28), BitConverter.ToSingle(data, 32), BitConverter.ToSingle(data, 36));

                // get entity
                entity = manager.entities[entity_id];

                // if does not exist, skip
                if (entity == null)
                {
                    Debug.LogError("Entity with id " + entity_id + " does not exist!");
                    break;
                }

                // update entity transform
                entity.transform.SetPositionAndRotation(position, rotation);
                entity.transform.localScale = scale;

                // end
                break;
            case 3:
                // unpack data
                int point_id = BitConverter.ToInt32(data, 0);
                Vector3 rotationOffset = new Vector3(BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
                float maxRayDistance = BitConverter.ToSingle(data, 16);
                int point_direction_id = data[20];
                bool rightController = data[21] == 1; // false for left, true for right

                // get controller
                GameObject controller = null;
                if (rightController) controller = manager.rightController;
                else controller = manager.leftController;

                // prepare ray
                Vector3 rayDirection = controller.transform.TransformDirection(Vector3.forward) + rotationOffset;
                RaycastHit hit;

                // point data
                Vector3 point = controller.transform.position + (rayDirection * maxRayDistance);
                Vector3 pointRotation = Vector3.down;

                // create raycast point
                if (Physics.Raycast(
                    controller.transform.position,
                    rayDirection, 
                    out hit, 
                    maxRayDistance)
                ) {
                    // if hit, create point at location with proper rotation
                    point = hit.point;
                    if (point_direction_id == 0)
                        pointRotation = rayDirection;
                    else if (point_direction_id == 1)
                        pointRotation = hit.normal;
                }

                // handle point directions 2-6
                if (point_direction_id == 2)
                    pointRotation = Vector3.forward;
                else if (point_direction_id == 3)
                    pointRotation = Vector3.back;
                else if (point_direction_id == 4)
                    pointRotation = Vector3.right;
                else if (point_direction_id == 5)
                    pointRotation = Vector3.left;
                else if (point_direction_id == 6)
                    pointRotation = Vector3.up;

                // save point and its rotation
                manager.points[point_id] = new KeyValuePair<Vector3, Vector3>(point, pointRotation);

                // end
                break;
            case 4:
                // unpack data
                entity_id = BitConverter.ToInt32(data, 0);
                point_id = BitConverter.ToInt32(data, 4);
                bool usePointDirection = data[0] == 1;
                rotationOffset = new Vector3(BitConverter.ToSingle(data, 9), BitConverter.ToSingle(data, 13), BitConverter.ToSingle(data, 17));

                // if neither entities or point maps have keys, throw error and break
                if (!manager.entities.ContainsKey(entity_id) || !manager.points.ContainsKey(point_id))
                {
                    Debug.LogError("Either point " + point_id + " or entity " + entity_id + " does not exist!");
                    break;
                }

                // get point and entity
                entity = manager.entities[entity_id];
                var pointPair = manager.points[point_id];

                // teleport entity
                entity.transform.position = pointPair.Key;
                if (usePointDirection) entity.transform.localRotation = Quaternion.Euler(pointPair.Value + rotationOffset);

                // end
                break;
            default:
                Debug.Log("No instruction made for id " + instruction.instructionID);
                break;
        }
    }
}