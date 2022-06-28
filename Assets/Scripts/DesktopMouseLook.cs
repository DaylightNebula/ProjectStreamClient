using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopMouseLook
{
    // game object variables
    GameObject headset, leftController, rightController;

    // mouse look variables
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0F;
    float rotationY = 0F;
    Quaternion originalRotation;

    // Start is called before the first frame update
    public void Start(GameObject headset, GameObject leftController, GameObject rightController)
    {
        // set game object variables
        this.headset = headset;
        this.leftController = leftController;
        this.rightController = rightController;

        // update original rotation
        originalRotation = headset.transform.localRotation;

        // move left and right controllers to the headset
        leftController.transform.SetPositionAndRotation(headset.transform.position, originalRotation);
        rightController.transform.SetPositionAndRotation(headset.transform.position, originalRotation);

        // hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    bool useCursor = true;
    public void Update()
    {
        // cancel if 6 is pressed
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (useCursor)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                useCursor = false;
            } else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                useCursor = true;
            }
        }

        // only continue if we should be using the cursor
        if (!useCursor) return;

        // Read the mouse input axis
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationX = ClampAngle(rotationX, minimumX, maximumX);
        rotationY = ClampAngle(rotationY, minimumY, maximumY);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);

        // update rotations
        Quaternion newRotation = originalRotation * xQuaternion * yQuaternion;
        headset.transform.localRotation = newRotation;
        leftController.transform.localRotation = newRotation;
        rightController.transform.localRotation = newRotation;
    }

    public float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
         angle += 360F;
        if (angle > 360F)
         angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
