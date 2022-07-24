using System;
using System.Xml;
using UnityEngine;

public class ButtonStateCondition : Condition
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
                return Input.GetKeyUp((KeyCode)Enum.Parse(typeof(KeyCode), button, true));
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
                return Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), button, true));
        }
        else if (state == "while_down")
        {
            if (button.StartsWith("mouse"))
            {
                int buttonID = 0;
                if (button == "mouse left") buttonID = 0;
                else if (button == "mouse right") buttonID = 1;
                else if (button == "mouse middle") buttonID = 2;
                return Input.GetMouseButton(buttonID);
            }
            else
            {
                return Input.GetKey((KeyCode)Enum.Parse(typeof(KeyCode), button, true));
            }
        }
        else
        {
            Debug.LogWarning("No button state created for " + state + " in ButtonStateCondition");
            return false;
        }
    }
}