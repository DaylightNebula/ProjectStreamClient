using System.Xml;
using UnityEngine;

class IfInstruction: Instruction
{
    Condition[] conditions;
    Instruction[] instructions;

    public IfInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        XmlNodeList children = xml.ChildNodes;
        foreach (XmlNode child in children)
        {
            if (child.Name == "conditions")
                conditions = XMLDecoder.decodeConditions(manager, child);
            else if (child.Name == "instructions")
            {
                instructions = new Instruction[child.ChildNodes.Count];
                for (int i = 0; i < child.ChildNodes.Count; i++)
                    instructions[i] = XMLDecoder.decodeInstruction(manager, child.ChildNodes.Item(i));
            }
        }
    }

    public override void execute(Manager manager)
    {
        bool shouldExecute = true;

        foreach (Condition c in conditions)
        {
            if (!c.isConditionMet(manager))
            {
                shouldExecute = false;
                break;
            }
        }

        if (shouldExecute)
            foreach (Instruction i in instructions)
                i.execute(manager);
    }
}
class LoopWhileInstruciton : Instruction
{
    Condition[] conditions;
    Instruction[] instructions;
    float cap;

    public LoopWhileInstruciton(Manager manager, XmlNode xml) : base(manager, xml)
    {
        cap = XMLDecoder.decodeFloat(xml.Attributes["cap"], 1000f);

        XmlNodeList children = xml.ChildNodes;
        foreach (XmlNode child in children)
        {
            if (child.Name == "conditions")
                conditions = XMLDecoder.decodeConditions(manager, child);
            else if (child.Name == "instructions")
            {
                instructions = new Instruction[child.ChildNodes.Count];
                for (int i = 0; i < child.ChildNodes.Count; i++)
                    instructions[i] = XMLDecoder.decodeInstruction(manager, child.ChildNodes.Item(i));
            }
        }
    }

    public override void execute(Manager manager)
    {
        float counter = 0f;
        while (shouldExecute(manager) && counter < cap)
        {
            foreach (Instruction i in instructions)
                i.execute(manager);
            counter++;
        }
    }

    private bool shouldExecute(Manager manager)
    {
        bool shouldExecute = true;

        foreach (Condition c in conditions)
        {
            if (!c.isConditionMet(manager))
            {
                shouldExecute = false;
                break;
            }
        }
        return shouldExecute;
    }
}
class LoopInstruction : Instruction
{
    Instruction[] instructions;
    float loopCount;

    public LoopInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        loopCount = float.Parse(xml.Attributes["count"].Value);

        instructions = new Instruction[xml.ChildNodes.Count];
        for (int i = 0; i < xml.ChildNodes.Count; i++)
            instructions[i] = XMLDecoder.decodeInstruction(manager, xml.ChildNodes.Item(i));
    }

    public override void execute(Manager manager)
    {
        for (int i = 0; i < loopCount; i++)
            foreach (Instruction instruction in instructions)
                instruction.execute(manager);
    }
}