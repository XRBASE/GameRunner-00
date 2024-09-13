using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

/// <summary>
/// This class listens to user input and detect whether keyboard is used as primary input or a gamepad
/// </summary>
public class InputDetector
{
    public Action<InputType> onInputTypeChanged;
    private const string KEYBOARD_NAME = "Keyboard";
    private const string CONTROLLER_NAME = "XInputControllerWindows";
    

    public enum InputType
    {
        KeyboardMouse,
        Gamepad
    }
    
    public InputDetector()
    {
        InputUser.onChange += InputUserOnChange;
    }

    private void InputUserOnChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if (change == InputUserChange.DevicePaired && device != null)
        {
            switch (device.name)
            {
                case KEYBOARD_NAME:
                    onInputTypeChanged?.Invoke(InputType.KeyboardMouse);
                    break;
                case CONTROLLER_NAME:
                    onInputTypeChanged?.Invoke(InputType.Gamepad);
                    break;
            }
        }
    }

    
}