using System;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps {
    /// <summary>
    /// Input for interaction with objects (select and grab)
    /// </summary>
    public class InteractInput : InputMap {
        public Action onBtnInteract;
        public Action onClickInteract;
        
        private InputAction _clickInteractAction;
        private InputAction _btnInteractAction;

        public InteractInput(InputActionAsset actions) : base(actions, "Interact") {
            _clickInteractAction = _map.FindAction("ClickInteract");
            _btnInteractAction = _map.FindAction("BtnInteract");
            
            _clickInteractAction.canceled += OnClickInteract;
            _btnInteractAction.canceled += OnBtnInteract;
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

            onClickInteract?.Invoke();
        }

        private void OnBtnInteract(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !InputManager.Instance.Cursor.ControlEnabled)
                return;

            onBtnInteract?.Invoke();
        }
    }
}