using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
    /// <summary>
    /// player movement related input. 
    /// </summary>
    public class PlayerMoveInput : InputMap {
        public bool RunState {
            get { return _runState; }
            set {
                if (_runState == value)
                    return;

                _runState = value;
                runStateChanged?.Invoke(RunState);
            }
        }

        private bool _runState = false;

        public Action<Vector2> directionChanged;
        public Action<Vector3> moveToCursor;
        public Action<Vector3> teleportToCursor;

        public Action<float> turn;

        public Action<bool> runStateChanged;
        public Action jump;

        private bool _validDrag;

        public PlayerMoveInput(InputActionAsset actions) : base(actions, "PlayerMove") {
            InputManager.Instance.TypingInput.onTyping += OnPlayerTyping;
            InputManager.Instance.Cursor.leftUp += OnCursorUp;
            InputManager.Instance.Cursor.rightUp += OnRightCursorUp;

            _map["Move"].started += OnChangeMoveDirection;
            _map["Move"].performed += OnChangeMoveDirection;
            _map["Move"].canceled += OnChangeMoveDirection;

            _map["Turn"].started += OnTurn;
            _map["Turn"].performed += OnTurn;
            _map["Turn"].canceled += OnTurn;

            _map["Run"].started += OnRun;
            _map["Run"].canceled += OnRun;

            _map["Jump"].started += OnJump;
        }


        public override void Dispose() {
            if (InputManager.Disposed)
                return;
            InputManager.Instance.TypingInput.onTyping -= OnPlayerTyping;
            InputManager.Instance.Cursor.leftUp -= OnCursorUp;
            InputManager.Instance.Cursor.rightUp -= OnRightCursorUp;

            _map["Move"].started -= OnChangeMoveDirection;
            _map["Move"].performed -= OnChangeMoveDirection;
            _map["Move"].canceled -= OnChangeMoveDirection;

            _map["Turn"].started -= OnTurn;
            _map["Turn"].performed -= OnTurn;
            _map["Turn"].canceled -= OnTurn;

            _map["Run"].started -= OnRun;
            _map["Run"].canceled -= OnRun;

            _map["Jump"].started -= OnJump;

            directionChanged = null;
            moveToCursor = null;

            turn = null;

            runStateChanged = null;
            jump = null;

            base.Dispose();
        }

        private void OnPlayerTyping(bool isTyping) {
            directionChanged?.Invoke(Vector2.zero);
            turn?.Invoke(0f);
        }

        private void OnChangeMoveDirection(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            Vector3 dir = Vector3.zero;
            if (!context.canceled) {
                dir = context.ReadValue<Vector2>();
            }

            directionChanged?.Invoke(dir);
        }

        private void OnCursorUp() {
            //check if current raycast was a navigation hit, and invoke with world position of that hit.
            if (!InputManager.Instance.Cursor.Dragging &&
                InputManager.Instance.Raycaster.HitState == CursorRayCaster.RCHitState.Navigate) {
                moveToCursor?.Invoke(InputManager.Instance.Raycaster.CurHit.point);
            }
        }

        private void OnRightCursorUp() {
            //check if current raycast was a navigation hit, and invoke with world position of that hit.
            if (InputManager.Instance.teleportEnabled &&
                InputManager.Instance.Raycaster.HitState == CursorRayCaster.RCHitState.Navigate) {
                teleportToCursor?.Invoke(InputManager.Instance.Raycaster.CurHit.point);
            }
        }


        /// <summary>
        /// Turn left or right.
        /// </summary>
        /// <param name="dir">-1 for left, 1 for right.</param>
        private void OnTurn(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            turn?.Invoke(context.ReadValue<float>());
        }

        private void OnRun(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            if (context.started) {
                RunState = true;
            }
            else {
                RunState = false;
            }
        }

        private void OnJump(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;
            jump?.Invoke();
        }
    }
}