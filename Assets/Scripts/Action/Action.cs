using System.Collections;
using System.Collections.Generic;
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
        foreach (Instruction i in instructions)
            i.execute(manager, null);
    }
}
