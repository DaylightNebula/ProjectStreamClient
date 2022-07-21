using System.Xml;
using UnityEngine;

public class UserMoveInstruction : Instruction
{
    Vector2 move;
    bool useUserDirection;
    string multiplyAxis;

    public UserMoveInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        move = XMLDecoder.decodeVector(xml.Attributes["move"], new Vector2(0f, 0f));
        useUserDirection = XMLDecoder.decodeBoolean(xml.Attributes["use_user_direction"], true);
        multiplyAxis = XMLDecoder.decodeString(xml.Attributes["multiply_axis"], "");
    }

    public override void execute(Manager manager)
    {
        // get character controller
        CharacterController controller = manager.currentPlayer.GetComponent<CharacterController>();

        // create temporary move variable for changing
        Vector2 actualMove = move;

        // if we should use user direction
        if (useUserDirection)
        {
            actualMove = customMult(move.y, controller.transform.forward) + customMult(move.x, controller.transform.right);
        }

        // multiply by axis if needed
        if (multiplyAxis.Length > 0)
        {
            // get axis
            float axisValue = Input.GetAxis(multiplyAxis);

            // make sure axis exists
            if (axisValue == null)
                Debug.LogWarning("Could not find axis " + axisValue);
            else
                actualMove *= axisValue;
        }

        // apply time to temp move
        actualMove *= Time.deltaTime;

        // apply move
        controller.Move(new Vector3(actualMove.x, 0f, actualMove.y));
    }

    private Vector2 customMult(float a, Vector3 three)
    {
        Vector2 b = new Vector2(three.x, three.z);
        return b * a;
    }
}
public class UserAddVelocityInstruction: Instruction
{
    Vector3 vel;

    public UserAddVelocityInstruction(Manager manager, XmlNode xml) : base(manager, xml)
    {
        vel = XMLDecoder.decodeVector(xml.Attributes["velocity"], new Vector3(0f, 0f, 0f));
    }

    public override void execute(Manager manager)
    {
        KeyboardController controller = manager.currentPlayer.GetComponent<KeyboardController>();
        controller.velocity += vel;
    }
}