using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class InputManager
{
    Manager manager;
    InstructionManager instructionManager;
    public InputManager(Manager manager, InstructionManager instructionManager)
    {
        // setup vars
        this.manager = manager;
        this.instructionManager = instructionManager;

        // setup callback functions
        manager.aButton.started += aButtonPressed;
        manager.bButton.started += bButtonPressed;
        manager.xButton.started += xButtonPressed;
        manager.yButton.started += yButtonPressed;
        manager.lTriggerPress.started += lTriggerPressed;
        manager.lGripPress.started += lGripPressed;
        manager.lJoyPress.started += lJoyPressed;
        manager.rTriggerPress.started += rTriggerPressed;
        manager.rGripPress.started += rGripPressed;
        manager.rJoyPress.started += rJoyPressed;
        manager.aButton.canceled += aButtonReleased;
        manager.bButton.canceled += bButtonReleased;
        manager.xButton.canceled += xButtonReleased;
        manager.yButton.canceled += yButtonReleased;
        manager.lTriggerPress.canceled += lTriggerReleased;
        manager.lGripPress.canceled += lGripReleased;
        manager.lJoyPress.canceled += lJoyReleased;
        manager.rTriggerPress.canceled += rTriggerReleased;
        manager.rGripPress.canceled += rGripReleased;
        manager.rJoyPress.canceled += rJoyReleased;

        // enable callbacks
        manager.aButton.Enable();
        manager.bButton.Enable();
        manager.xButton.Enable();
        manager.yButton.Enable();
        manager.lTriggerPress.Enable();
        manager.lGripPress.Enable();
        manager.lJoyPress.Enable();
        manager.rTriggerPress.Enable();
        manager.rGripPress.Enable();
        manager.rJoyPress.Enable();
    }

    private void aButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.A_BUTTON); manager.SendButtonPress(6); }
    private void bButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.B_BUTTON); manager.SendButtonPress(7); }
    private void xButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.X_BUTTON); manager.SendButtonPress(8); }
    private void yButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.Y_BUTTON); manager.SendButtonPress(9); }
    private void lTriggerPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_TRIGGER); manager.SendButtonPress(0); }
    private void lGripPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_GRIP); manager.SendButtonPress(2); }
    private void lJoyPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_JOYSTICK); manager.SendButtonPress(4); }
    private void rTriggerPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_TRIGGER); manager.SendButtonPress(1); }
    private void rGripPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_GRIP); manager.SendButtonPress(3); }
    private void rJoyPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_JOYSTICK); manager.SendButtonPress(5); }

    private void aButtonReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(6); }
    private void bButtonReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(7); }
    private void xButtonReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(8); }
    private void yButtonReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(9); }
    private void lTriggerReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(0); }
    private void lGripReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(2); }
    private void lJoyReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(4); }
    private void rTriggerReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(1); }
    private void rGripReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(3); }
    private void rJoyReleased(InputAction.CallbackContext callbackContext) { manager.SendButtonRelease(5); }
}
