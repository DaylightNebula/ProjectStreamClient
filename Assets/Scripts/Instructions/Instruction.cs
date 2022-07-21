using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

public abstract class Instruction
{
    public Instruction(Manager manager, XmlNode xml) { }
    public abstract void execute(Manager manager);

    // STATIC
    public static Instruction getCompiledInstruction(Manager manager, XmlNode xml)
    {
        switch (xml.Name)
        {
            case "entity_spawn": return new EntitySpawnInstruction(manager, xml);
            case "entity_remove": return new EntitySpawnInstruction(manager, xml);
            case "entity_settransform": return new EntitySetTransformInstruction(manager, xml);
            case "entity_setactive": return new SetEntityActiveInstruction(manager, xml);
            case "entity_move": return new MoveEntityInstruction(manager, xml);
            case "point_createfromentityraycast": return new CreatePointFromRaycastInstruction(manager, xml);
            case "point_moveentity": return new MoveEntityToPointInstruction(manager, xml);
            case "sound_playfromentity": return new PlaySoundFromEntityInstruction(manager, xml);
            case "event_call": return new CallEventInstruction(manager, xml);
            case "rigidbody_applyforce": return new ApplyForceToRigidbodyInstruction(manager, xml);
            case "user_move": return new UserMoveInstruction(manager, xml);
            case "user_addvel": return new UserAddVelocityInstruction(manager, xml);
            case "var_set": return new SetVariableInstruction(manager, xml);
            case "var_mod": return new ModifyVariableInstruction(manager, xml);
            case "if": return new IfInstruction(manager, xml);
            case "loopwhile": return new LoopWhileInstruciton(manager, xml);
            case "loop": return new LoopInstruction(manager, xml);
            default:
                Debug.LogWarning("No instruction registered for " + xml.Name);
                return null;
        }
    }

    public static void runInstructions(Manager manager, InstructionContainer instructions)
    {
        foreach (Instruction i in instructions.instructions)
            i.execute(manager);
    }
}
public class InstructionContainer
{
    public string executeTime;
    public Instruction[] instructions;

    public InstructionContainer(string executeTime, Instruction[] instructions)
    {
        this.executeTime = executeTime;
        this.instructions = instructions;
    }
}
