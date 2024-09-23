using System;
using UnityEngine;

namespace Cohort.GameRunner.Input.Actions
{
    /// <summary>
    /// This class reads the gamepad input for the Xbox controller and maps it to the according actions. This uses the old unity input system
    /// </summary>
    public class GamepadInput
    {
        private const string RIGHT_JOYSTICK_AXIS_X_NAME = "RightJoyStickX";
        private const string RIGHT_JOYSTICK_AXIS_Y_NAME = "RightJoyStickY";
        private const string LEFT_JOYSTICK_AXIS_X_NAME = "LeftJoyStickX";
        private const string LEFT_JOYSTICK_AXIS_Y_NAME = "LeftJoyStickY";
        private const string D_PAD_AXIS_X_NAME = "DPadX";
        private const string D_PAD_AXIS_Y_NAME = "DPadY";

        public Action<Vector2> rightJostickAxisChanged;
        public Action<Vector2> leftJoystickAxisChanged;
        public Action<Vector2> leftDPadAxisChanged;
        public Action<bool> rightRightButtonPressed;
        public Action<bool> rightDownButtonPressed;
        public Action<bool> rightLeftButtonPressed;

        private AxisInput _leftJoystickAxis, _rightJoystickAxis, _dPadAxis;
        private ButtonInput _rightRightButton, _rightDownButton, _rightLeftButton;

        private bool _leftJoystickAxisActive, _rightJoyStickAxisActive, _dPadAxisActive;

        public GamepadInput()
        {
            _leftJoystickAxis = new AxisInput(LEFT_JOYSTICK_AXIS_X_NAME, LEFT_JOYSTICK_AXIS_Y_NAME);
            _rightJoystickAxis = new AxisInput(RIGHT_JOYSTICK_AXIS_X_NAME, RIGHT_JOYSTICK_AXIS_Y_NAME);
            _dPadAxis = new AxisInput(D_PAD_AXIS_X_NAME, D_PAD_AXIS_Y_NAME);
            _rightRightButton = new ButtonInput(new [] {KeyCode.Joystick1Button1, KeyCode.Joystick2Button1, KeyCode.Joystick3Button1, KeyCode.Joystick4Button1, KeyCode.Joystick5Button1, KeyCode.Joystick6Button1, KeyCode.Joystick7Button1, KeyCode.Joystick8Button1});
            _rightDownButton = new ButtonInput(new [] {KeyCode.Joystick1Button0, KeyCode.Joystick2Button0, KeyCode.Joystick3Button0, KeyCode.Joystick4Button0, KeyCode.Joystick5Button0, KeyCode.Joystick6Button0, KeyCode.Joystick7Button0, KeyCode.Joystick8Button0});
            _rightLeftButton = new ButtonInput(new [] {KeyCode.Joystick1Button2, KeyCode.Joystick2Button2, KeyCode.Joystick3Button2, KeyCode.Joystick4Button2, KeyCode.Joystick5Button2, KeyCode.Joystick6Button2, KeyCode.Joystick7Button2, KeyCode.Joystick8Button2});
        }

        ~GamepadInput()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (InputManager.Disposed)
                return;
            
            rightJostickAxisChanged = null;
            leftJoystickAxisChanged = null;
            leftDPadAxisChanged = null;
            rightRightButtonPressed = null;
            rightDownButtonPressed = null;
            rightDownButtonPressed = null;
        }
        
        /// <summary>
        /// Only update the input when an Xbox Controller is connected
        /// </summary>
        public void Update() {
            _leftJoystickAxisActive = ProcessAxisEvent(_leftJoystickAxis, leftJoystickAxisChanged, _leftJoystickAxisActive);
            _rightJoyStickAxisActive = ProcessAxisEvent(_rightJoystickAxis, rightJostickAxisChanged, _rightJoyStickAxisActive);
            _dPadAxisActive = ProcessAxisEvent(_dPadAxis, leftDPadAxisChanged, _dPadAxisActive);
            
            ProcessButtonEvent(_rightDownButton, rightDownButtonPressed);
            ProcessButtonEvent(_rightRightButton, rightRightButtonPressed);
            ProcessButtonEvent(_rightLeftButton, rightLeftButtonPressed);
        }


        private void ProcessButtonEvent(ButtonInput buttonInput, Action<bool> onButtonPressed)
        {
            if (buttonInput.ReadButtonVal(out bool state))
            {
                onButtonPressed?.Invoke(state);
            }
        }
        
        private bool ProcessAxisEvent(AxisInput axisInput, Action<Vector2> axisChanged, bool active)
        {
            if (axisInput.GetAxis(out Vector2 axis))
            {
                axisChanged?.Invoke(axis);
                active = true;
            }else if (active)
            {
                axisChanged?.Invoke(Vector2.zero);
                active = false;
            }
            return active;
        }

        /// <summary>
        /// Executes the subscribed events with the default values in case the controller disconnects 
        /// </summary>
        private void ResetInputs()
        {
            leftJoystickAxisChanged?.Invoke(Vector2.zero);
            rightJostickAxisChanged?.Invoke(Vector2.zero);
            leftDPadAxisChanged?.Invoke(Vector2.zero);
            
            rightRightButtonPressed?.Invoke(false);
            rightDownButtonPressed?.Invoke(false);
            rightLeftButtonPressed?.Invoke(false);
        }
    }
}