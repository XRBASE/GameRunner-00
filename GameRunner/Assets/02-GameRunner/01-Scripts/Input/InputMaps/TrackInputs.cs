using System;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps
{
    public class TrackInputs : InputMap
    {
        public Action<float> trackDirectionChanged;
        public Action cancelTrack;
        
        private InputAction _trackDirectionAction;
        private InputAction _cancelTrack;
        private InputAction _trackDirectionCursorAction;
        
        private bool _isUsingKeyboard;

        public TrackInputs(InputActionAsset actions) : base(actions, "Track")
        {
            _trackDirectionAction = _map.FindAction("TrackDirection");
            _cancelTrack = _map.FindAction("CancelTrack");
            _trackDirectionCursorAction = _map.FindAction("TrackDirectionCursor");
            
            _cancelTrack.started += OnCancelTrack;
            
            _trackDirectionAction.started += OnTrackDirChanged;
            _trackDirectionAction.performed += OnTrackDirChanged;
            _trackDirectionAction.canceled += OnTrackDirChanged;
            
            _trackDirectionCursorAction.started += OnTrackDirCursorChanged;
            _trackDirectionCursorAction.performed += OnTrackDirCursorChanged;
            _trackDirectionCursorAction.canceled += OnTrackDirCursorChanged;
        }
        
        public override void Dispose()
        {
            if (!DataServices.Login.UserLoggedIn)
                return;
            
            _trackDirectionAction.started -= OnTrackDirChanged;
            _trackDirectionAction.performed -= OnTrackDirChanged;
            _trackDirectionAction.canceled -= OnTrackDirChanged;
            
            _trackDirectionCursorAction.started -= OnTrackDirCursorChanged;
            _trackDirectionCursorAction.performed -= OnTrackDirCursorChanged;
            _trackDirectionCursorAction.canceled -= OnTrackDirCursorChanged;
            
            _trackDirectionAction.Dispose();
            trackDirectionChanged = null;
            
            _trackDirectionCursorAction.Dispose();
            _trackDirectionCursorAction = null;
            
            _cancelTrack.started -= OnCancelTrack;
            
            _cancelTrack.Dispose();
            cancelTrack = null;
            
            base.Dispose();
        }
        
        private void OnPlayerTyping(bool isTyping) {
            trackDirectionChanged?.Invoke(0f);
        }

        private void OnCancelTrack(InputAction.CallbackContext obj)
        {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            cancelTrack?.Invoke();
        }

        private void OnTrackDirChanged(InputAction.CallbackContext context)
        {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn )
                return;
            if (context.performed || context.started)
            {
                _isUsingKeyboard = true;
            }
            else
            {
                _isUsingKeyboard = false;
            }
            float direction = context.ReadValue<float>();
            trackDirectionChanged?.Invoke(direction);
        }
        
        private void OnTrackDirCursorChanged(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn || !InputManager.Instance.GameCursor.ControlEnabled || _isUsingKeyboard)
                return;
            
            float direction = context.ReadValue<float>();
            if (InputManager.Instance.Raycaster.PointerOverUI)
                direction = 0f;
            trackDirectionChanged?.Invoke(direction);
        }
    }
}