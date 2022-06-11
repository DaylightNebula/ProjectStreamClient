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

    private void aButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.A_BUTTON); }
    private void bButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.B_BUTTON); }
    private void xButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.X_BUTTON); }
    private void yButtonPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.Y_BUTTON); }
    private void lTriggerPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_TRIGGER); }
    private void lGripPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_GRIP); }
    private void lJoyPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.L_JOYSTICK); }
    private void rTriggerPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_TRIGGER); }
    private void rGripPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_GRIP); }
    private void rJoyPressed(InputAction.CallbackContext callbackContext) { instructionManager.buttonPressed(InstructionButton.R_JOYSTICK); }
}
