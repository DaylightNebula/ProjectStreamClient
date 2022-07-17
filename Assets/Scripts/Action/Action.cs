using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Action
{
    string name;
    Condition[] conditions;
    Instruction[] instructions;

    public Action(string name, Condition[] conditions, Instruction[] instructions) { this.name = name; this.conditions = conditions; this.instructions = instructions; }

    public bool canExecute(Manager manager)
    {
        bool output = true;

        // loop through conditions, if any are not met, set output to false
        foreach (Condition condition in conditions)
        {
            if (condition != null && !condition.isConditionMet(manager))
                output = false;
        }

        return output;
    }

    public void execute(Manager manager)
    {
        // call all instructions in this action
        foreach (Instruction i in instructions)
            i.execute(manager, null);

        // tell behavior server that this action was called if this is not an update action
        if (name == "#update") return;
        int[] nameLength = new int[] { name.Length };
        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        byte[] packet = new byte[4 + name.Length];
        Buffer.BlockCopy(nameLength, 0, packet, 0, 4);
        Buffer.BlockCopy(nameBytes, 0, packet, 4, nameBytes.Length);
        manager.behaviorClient.sendPacket(0x09, packet);
    }
}
