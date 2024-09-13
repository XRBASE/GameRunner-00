using System;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
    /// <summary>
    /// Input for interaction with objects (select and grab)
    /// </summary>
    public class InteractInput : InputMap {
        public Action onProximityInteract;
        public Action onInteract;
        private InputAction _clickInteractAction;
        private InputAction _proximityInteractAction;

        public InteractInput(InputActionAsset actions) : base(actions, "Interact") {
            _clickInteractAction = _map.FindAction("ClickInteract");
            _proximityInteractAction = _map.FindAction("ProximityInteract");
            _proximityInteractAction.performed += ProximityInteractAction;
            _clickInteractAction.canceled += OnClickInteract;
        }

        private void ProximityInteractAction(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn)
                return;
            onProximityInteract?.Invoke();
        }

        public override void Dispose() {
            _clickInteractAction.started -= OnClickInteract;
            _clickInteractAction.Dispose();
        }

        private void OnClickInteract(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !InputManager.Instance.Cursor.ControlEnabled ||
                InputManager.Instance.Cursor.Dragging ||
                InputManager.Instance.Raycaster.HitState != CursorRayCaster.RCHitState.Interact)
                return;

            onInteract?.Invoke();
        }
    }
}