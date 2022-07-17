using System.Xml;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Condition
{
    public Condition(Manager manager, XmlNode xml) {}
    public abstract bool isConditionMet(Manager manager);

    // STATIC
    public static Condition getCompiledCondition(Manager manager, XmlNode xml)
    {
        switch(xml.Name)
        {
            case "buttonstate":
                return new ButtonStateCondition(manager, xml);
            default:
                Debug.LogWarning("No condition made for " + xml.Name);
                return null;
        }
    }
}
public class OnUpdateCondition: Condition
{
    public OnUpdateCondition() : base(null, null) { }

    public override bool isConditionMet(Manager manager)
    {
        return true;
    }
}
public class ButtonStateCondition: Condition
{
    string button;
    string state;

    public ButtonStateCondition(Manager manager, XmlNode xml) : base(manager, xml) 
    {
        button = xml.Attributes["button"].Value;
        state = XMLDecoder.decodeString(xml.Attributes["state"], "up");
    }

    public override bool isConditionMet(Manager manager)
    {
        if (state == "up")
        {
            if (button.StartsWith("mouse"))
            {
                int buttonID = 0;
                if (button == "mouse left") buttonID = 0;
                else if (button == "mouse right") buttonID = 1;
                else if (button == "mouse middle") buttonID = 2;
                return Input.GetMouseButtonUp(buttonID);
            } 
            else
                return Input.GetKeyUp((KeyCode)Enum.Parse(typeof(KeyCode), button));
        }
        else if (state == "down")
        {
            if (button.StartsWith("mouse"))
            {
                int buttonID = 0;
                if (button == "mouse left") buttonID = 0;
                else if (button == "mouse right") buttonID = 1;
                else if (button == "mouse middle") buttonID = 2;
                return Input.GetMouseButtonDown(buttonID);
            }
            else
                return Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), button));
        }
        else {
            Debug.LogWarning("No button state created for " + state + " in ButtonStateCondition");
            return false;
        }
    }
}