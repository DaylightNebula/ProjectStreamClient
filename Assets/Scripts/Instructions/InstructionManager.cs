using System;
using System.Collections.Generic;
using UnityEngine;

public class InstructionManager
{
    Manager manager;

    // instruction lists
    InstructionData[] assetServerConnectInstructions;
    InstructionData[] whenRunningInstructions;
    InstructionData[][] onJoystickChangeInstructions = new InstructionData[2][];
    InstructionData[][] whenJoystickNotZeroInstructions = new InstructionData[2][];
    InstructionData[][] onTriggerChangeInstructions = new InstructionData[2][];
    InstructionData[][] whenTriggerNotZeroInstructions = new InstructionData[2][];
    InstructionData[][] onGripChangeInstructions = new InstructionData[2][];
    InstructionData[][] whenGripNotZeroInstructions = new InstructionData[2][];
    InstructionData[][] onButtonPress = new InstructionData[11][];
    InstructionData[][] onButtonReleased = new InstructionData[11][];

    // instructions
    public CreateEntityInstruction createEntityInstruction = new CreateEntityInstruction();
    public RemoveEntityInstruction removeEntityInstruction = new RemoveEntityInstruction();
    public UpdateEntityTransformInstruction updateEntityTransformInstruction = new UpdateEntityTransformInstruction();
    public CreateRaycastPointInstruction createRaycastPointInstruction = new CreateRaycastPointInstruction();
    public MoveEntityToPointInstruction moveEntityToPointInstruction = new MoveEntityToPointInstruction();
    public CreatePointFromMidPointOfTwoPointsInstruction createPointFromMidPointOfTwoPointsInstruction = new CreatePointFromMidPointOfTwoPointsInstruction();
    public MoveEntityWithTimeInstruction MoveEntityWithTimeInstruction = new MoveEntityWithTimeInstruction();
    public SetEntityCollideableInstruction setEntityCollideableInstruction = new SetEntityCollideableInstruction();
    public ChangeEntityVisibilityInstruction changeEntityVisibilityInstruction = new ChangeEntityVisibilityInstruction();
    public CreateParticleEmitterInstruction createParticleEmitterInstruction = new CreateParticleEmitterInstruction();
    public CreateLightInstruction createLightInstruction = new CreateLightInstruction();
    public CallEventInstruction callEventInstruction = new CallEventInstruction();
    public LoadAssetInstruction loadAssetInstruction = new LoadAssetInstruction();

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
    public void buttonPressed(InstructionButton button) { ExecuteInstructions(onButtonPress[(int)button]); }
    public void buttonReleased(InstructionButton button) { ExecuteInstructions(onButtonReleased[(int)button]); }

    // function to apply instructions lists
    public void applyInstructionList(int execute_id, int execute_data, InstructionData[] instructions)
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
            case 10:
                onButtonReleased[execute_data] = instructions;
                break;
            default:
                Debug.Log("No when execute function made for id " + execute_id);
                break;
        }
    }

    // functions to execute instructions
    public void ExecuteInstructions(InstructionData[] instructions)
    {
        if (instructions == null) return;
        foreach (InstructionData instruction in instructions)
        {
            ExecuteInstruction(instruction);
        }
    }

    public void ExecuteInstruction(InstructionData inData)
    {
        // execute instruction
        Instruction instruction = Instruction.instructions[inData.instructionID];
        if (instruction != null) instruction.execute(manager, inData.instructionData);
    }
}
