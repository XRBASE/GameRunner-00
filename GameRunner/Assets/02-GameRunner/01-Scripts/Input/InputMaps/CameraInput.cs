using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
    /// <summary>
    /// Camera goes BRRRRRR.
    /// </summary>
    public class CameraInput : InputMap {
        /// <summary>
        /// zooms the cam.
        /// </summary>
        public Action<Vector2, bool> zoom;

        /// <summary>
        /// Pans/rotates the cam.
        /// </summary>
        public Action<Vector2, ActionState> pan;

        /// <summary>
        /// Go to numbered viewpoint (if exists and active).
        /// </summary>
        public Action<int> gotoNum;

        public Action gotoDefaultCamera;

        private InputAction _zoomAction;

        private InputAction _leftMousePanAction;

        private InputAction _rightMousePanAction;

        //checks starting position for drag and determines whether it's a valid camera drag
        private bool _validDrag;

        private InputAction _gotoNumAction;

        private InputAction _gotoDefaultCamera;



        public CameraInput(InputActionAsset actions) : base(actions, "Camera") {
            _zoomAction = _map.FindAction("ZoomIn");
            _leftMousePanAction = _map.FindAction("LeftMousePan");
            _rightMousePanAction = _map.FindAction("RightMousePan");
            _gotoNumAction = _map.FindAction("GotoNum");
            _gotoDefaultCamera = _map.FindAction("GotoDefaultCamera");

            _zoomAction.performed += OnZoom;
            _gotoNumAction.started += OnGotoNum;

            _gotoDefaultCamera.performed += OnGotoDefaultCamera;
            _leftMousePanAction.started += OnMousePan;
            _leftMousePanAction.performed += OnMousePan;
            _leftMousePanAction.canceled += OnMousePan;
            _rightMousePanAction.started += OnMousePan;
            _rightMousePanAction.performed += OnMousePan;
            _rightMousePanAction.canceled += OnMousePan;
        }

        private void OnGotoDefaultCamera(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || InputManager.Instance.TypingInput.PlayerTyping)
                return;

            gotoDefaultCamera?.Invoke();
        }

        public override void Dispose() {
            _zoomAction.performed -= OnZoom;
            _zoomAction.Dispose();
            zoom = null;

            _leftMousePanAction.started -= OnMousePan;
            _leftMousePanAction.performed -= OnMousePan;
            _leftMousePanAction.canceled -= OnMousePan;
            _rightMousePanAction.started -= OnMousePan;
            _rightMousePanAction.performed -= OnMousePan;
            _rightMousePanAction.canceled -= OnMousePan;
            _gotoDefaultCamera.performed -= OnGotoDefaultCamera;

            _rightMousePanAction.Dispose();
            _leftMousePanAction.Dispose();
            pan = null;

            _gotoNumAction.started -= OnGotoNum;
            _gotoNumAction.Dispose();
        }

        private void OnZoom(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !InputManager.Instance.Cursor.ControlEnabled)
                return;

            Vector2 dir = context.ReadValue<Vector2>();
            zoom?.Invoke(-dir, context.performed || context.started);
        }

        private void OnMousePan(InputAction.CallbackContext context) {
            if (!InputManager.Instance.Cursor.ControlEnabled || (!context.started && !_validDrag) ||
                !DataServices.Login.UserLoggedIn)
                return;

            if (context.started) {
                _validDrag = !InputManager.Instance.Raycaster.PointerOverUI;
                if (_validDrag) {
                    pan?.Invoke(Vector2.zero, ActionState.Started);
                }
            }
            else if (context.performed) {
                pan?.Invoke(context.ReadValue<Vector2>(), ActionState.Active);
            }
            else {
                pan?.Invoke(Vector2.zero, ActionState.Canceled);
            }
        }

        private void OnGotoNum(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            int keyNum = (int)context.ReadValue<float>();
            gotoNum?.Invoke(keyNum);
        }
    }
}