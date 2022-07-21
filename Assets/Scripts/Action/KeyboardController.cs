using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController: MonoBehaviour
{
    // variables controlling falling and on ground
    public float groundDistance = 0.05f;
    public bool canFall = false;
    public bool onGround = false;

    // mouse look variables
    public GameObject camera;
    public float sensitivityX = 10F;
    public float sensitivityY = 10F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -60F;
    public float maximumY = 60F;
    float rotationX = 0F;
    float rotationY = 0F;

    public Vector3 velocity = new Vector3(0f, 0f, 0f);
    CharacterController controller;
    public float gravity = -9.81f;
    public float xzDrag = 0.1f;

    void Start()
    {
        // hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller = GetComponent<CharacterController>();
    }

    bool useCursor = true;
    void Update()
    {
        // cancel if F1 is pressed and we are in the editor
        if (Input.GetKeyDown(KeyCode.F1) && Application.isEditor)
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
        transform.localRotation = xQuaternion;
        camera.transform.localRotation = yQuaternion;

        // check if can fall
        RaycastHit hit;
        canFall = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity);
        if (!canFall && velocity.y < 0f)
            velocity.y = 0f;

        // if we can fall, check ground distance
        onGround = hit.distance < groundDistance;

        // apply gravity to velocity
        if (canFall && !onGround) velocity.y += gravity * Time.deltaTime;

        // if on ground and velocity is going down, set velocity to 0
        if (onGround && velocity.y < 0f) velocity.y = 0f;

        // apply velocity
        controller.Move(velocity * Time.deltaTime);
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
