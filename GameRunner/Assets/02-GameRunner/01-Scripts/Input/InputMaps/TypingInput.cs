using System;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
    public class TypingInput : InputMap {
        public bool PlayerTyping {
            get { return _typing; }
            set {
                if (_typing == value)
                    return;

                _typing = value;
                onTyping?.Invoke(_typing);
            }
        }

        private bool _typing;
        public Action<bool> onTyping;

        public TypingInput(InputActionAsset actions) : base(actions, "Typing") {
            actions["Escape"].started += OnStopTyping;
            actions["Escape"].canceled += OnStopTyping;
        }

        private void OnStopTyping(InputAction.CallbackContext obj) {
            PlayerTyping = false;
        }
    }
}