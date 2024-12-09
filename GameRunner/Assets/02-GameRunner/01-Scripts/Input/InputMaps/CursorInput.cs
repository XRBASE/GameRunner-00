using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps {
    /// <summary>
    /// Used only for generic cursor information, not for actual input.
    /// CursorRaycaster will use this information to track the position on screen for instance, that position will in respect
    /// trigger the actual input like PlayerMove.MoveToPosition
    /// </summary>
    public class CursorInput : InputMap {
        public const float CLICK_TIME_THRESHOLD = 0.3f;

        public bool ControlEnabled {
            get { return _controlEnabled; }
            set { _controlEnabled = value; }
        }

        private bool _controlEnabled = true;

        public Vector2 ScreenPosition { get; private set; }
        public bool Dragging { get; private set; }
        public bool LeftDown { get; private set; }
        public bool RightDown { get; private set; }

        //used for UI calls that also trigger when the button is clicked or released outside of the UI element.
        public Action leftDown;
        public Action leftUp;

        public Action rightDown;
        public Action rightUp;
        private float _rightClickTimer;

        private InputAction _screenPosAction;
        private InputAction _dragAction;

        private InputAction _leftClick;
        private InputAction _rightClick;

        public CursorInput(InputActionAsset actions, bool isGameInput) : base(actions, isGameInput?"GameCursor":"LearningCursor") {
            _screenPosAction = _map.FindAction("ScreenPosition");
            _screenPosAction.performed += OnScreenPosChanged;

            _dragAction = _map.FindAction("Drag");
            _dragAction.started += OnDrag;
            _dragAction.performed += OnDrag;
            _dragAction.canceled += OnDrag;

            _leftClick = _map.FindAction("LeftClick");
            _leftClick.started += OnLeftClick;
            _leftClick.canceled += OnLeftClick;

            _rightClick = _map.FindAction("RightClick");
            _rightClick.started += OnRightClick;
            _rightClick.canceled += OnRightClick;
        }

        public override void Dispose() {
            _screenPosAction.performed -= OnScreenPosChanged;
            _screenPosAction.Dispose();

            _dragAction.started -= OnDrag;
            _dragAction.performed -= OnDrag;
            _dragAction.canceled -= OnDrag;
            _dragAction.Dispose();

            _leftClick.started -= OnLeftClick;
            _leftClick.canceled -= OnLeftClick;
            _leftClick.Dispose();

            _rightClick.started -= OnRightClick;
            _rightClick.canceled -= OnRightClick;
            _rightClick.Dispose();

            leftDown = null;
            leftUp = null;

            rightDown = null;
            rightUp = null;

            base.Dispose();
        }

        private void OnScreenPosChanged(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !ControlEnabled)
                return;

            ScreenPosition = context.ReadValue<Vector2>();
        }

        private void OnDrag(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !ControlEnabled)
                return;

            if (context.performed) {
                //set to false on mouse down to ensure it is true only while dragging, even in the mouseUp event.
                Dragging = true;
            }
        }

        private void OnLeftClick(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !ControlEnabled)
                return;

            if (context.started) {
                //this disables any drag on start, so that if timer or threshold is not reached, it will count as a click, not a drag
                leftDown?.Invoke();
                Dragging = false;
                LeftDown = true;
            }
            else if (context.canceled) {
                leftUp?.Invoke();
                Dragging = false;
                LeftDown = false;
            }
        }

        private void OnRightClick(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !ControlEnabled)
                return;

            if (context.started) {
                //this disables any drag on start, so that if timer or threshold is not reached, it will count as a click, not a drag
                _rightClickTimer = Time.realtimeSinceStartup;
                rightDown?.Invoke();
                RightDown = true;
            }
            else if (context.canceled) {
                if (Time.realtimeSinceStartup - _rightClickTimer <= CLICK_TIME_THRESHOLD) {
                    rightUp?.Invoke();
                }

                RightDown = false;
            }
        }
    }
}