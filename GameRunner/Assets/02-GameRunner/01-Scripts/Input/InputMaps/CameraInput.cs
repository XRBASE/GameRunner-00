using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps {
    /// <summary>
    /// Camera goes BRRRRRR.
    /// </summary>
    public class CameraInput : InputMap {
        /// <summary>
        /// zooms the cam.
        /// </summary>
        public Action<float> zoom;
        
        public CameraInput(InputActionAsset actions) : base(actions, "Camera") {
            _map["ZoomIn"].performed += OnZoom;
        }

        public override void Dispose() {
            _map["ZoomIn"].performed -= OnZoom;
            zoom = null;
        }

        private void OnZoom(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !InputManager.Instance.GameCursor.ControlEnabled)
                return;

            Vector2 dir = context.ReadValue<Vector2>();
            zoom?.Invoke(-dir.y);
        }
    }
}