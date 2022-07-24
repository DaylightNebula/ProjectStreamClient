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
public class PlatformCondition: Condition
{
    string targetPlatform;

    public PlatformCondition(Manager manager, XmlNode xml): base(manager, xml)
    {
        targetPlatform = xml.Attributes["platform"].Value;
    }

    public override bool isConditionMet(Manager manager)
    {
        return targetPlatform.Equals(manager.platform, StringComparison.OrdinalIgnoreCase);
    }
}