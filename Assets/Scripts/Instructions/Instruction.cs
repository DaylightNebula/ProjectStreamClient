using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Instruction
{
    // stuff for tracking instructions
    public static Dictionary<int, Instruction> instructions = new Dictionary<int, Instruction>();
    public Instruction()
    {
        Instruction.instructions.Add(getID(), this);
    }

    // abstract stuff for creating new instructions
    public abstract int getID();
    public abstract void execute(Manager manager, byte[] data);
}
public struct InstructionData
{
    public InstructionData(int id, int byteLength, byte[] data)
    {
        instructionID = id;
        instructionByteLength = byteLength;
        instructionData = data;
    }

    public int instructionID { get; }
    public int instructionByteLength { get; }
    public byte[] instructionData { get; }
}
public enum InstructionButton
{
    L_TRIGGER = 0x00,
    R_TRIGGER = 0x01,
    L_GRIP = 0x02,
    R_GRIP = 0x03,
    L_JOYSTICK = 0x04,
    R_JOYSTICK = 0x05,
    A_BUTTON = 0x06,
    B_BUTTON = 0x07,
    X_BUTTON = 0x08,
    Y_BUTTON = 0x09,
    SYSTEM_BUTTON = 0x0A,
    MENU_BUTTON = 0x0B
}
public enum InstructionController
{
    LEFT = 0x00,
    RIGHT = 0x01,
    HMD = 0x02
}