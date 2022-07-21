using System.Xml;

public class SetCameraEffectsInstruction : Instruction
{
    XmlNode xml;

    public SetCameraEffectsInstruction(Manager manager, XmlNode xml): base(manager, xml)
    {
        this.xml = xml;
    }

    public override void execute(Manager manager)
    {
        manager.effectManager.setEffects(xml);
    }
}
public class AddCameraEffectsInstruction : Instruction
{
    XmlNode xml;

    public AddCameraEffectsInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        this.xml = xml;
    }

    public override void execute(Manager manager)
    {
        manager.effectManager.addEffects(xml);
    }
}
public class ClearCameraEffectsInstruction : Instruction
{
    public ClearCameraEffectsInstruction(Manager manager, XmlNode xml) : base(manager, xml) {}

    public override void execute(Manager manager)
    {
        manager.effectManager.clearEffects();
    }
}