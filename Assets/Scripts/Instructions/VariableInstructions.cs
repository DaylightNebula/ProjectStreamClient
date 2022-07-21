using System.Xml;
using UnityEngine;

class SetVariableInstruction: Instruction
{
    string variableName;
    float value;

    public SetVariableInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        variableName = xml.Attributes["name"].Value;
        value = float.Parse(xml.Attributes["value"].Value);
    }

    public override void execute(Manager manager)
    {
        manager.setVariable(variableName, value);
        Debug.Log("Set " + variableName + " to " + value);
    }
}
class ModifyVariableInstruction: Instruction
{
    string variableName;
    string operation;
    float value;

    public ModifyVariableInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        variableName = xml.Attributes["name"].Value;
        operation = xml.Attributes["operation"].Value;
        value = float.Parse(xml.Attributes["value"].Value);
    }

    public override void execute(Manager manager)
    {
        // get variable
        float variable = manager.getVariable(variableName);

        // modify variable based on operation
        switch(operation)
        {
            case "add": variable += value; break;
            case "subtract": variable -= value; break;
            case "multiply": variable *= value; break;
            case "divide": variable /= value; break;
            default:
                Debug.LogError("Unknown variable modify operation " + operation);
                break;
        }

        // save variable
        manager.setVariable(variableName, variable);
        Debug.Log("Modified " + variableName + " to " + variable + " with operation " + operation);
    }
}