using System;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
    /// <summary>
    /// Input for interaction with objects (select and grab)
    /// </summary>
    public class EmoteInput : InputMap {
        private InputAction _emoteAction;

        /// <summary>
        /// int for emote id and bool for networking the emote 
        /// </summary>
        public Action<int> emote;

        public EmoteInput(InputActionAsset actions) : base(actions, "Emote") {
            _emoteAction = _map.FindAction("Emote");
            _emoteAction.started += OnEmote;
        }

        public override void Dispose() {
            _emoteAction.started -= OnEmote;
            _emoteAction.Dispose();
            emote = null;
        }

        private void OnEmote(InputAction.CallbackContext context) {
            if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
                return;

            int keyNum = (int)context.ReadValue<float>();
            emote?.Invoke(keyNum);
        }
    }
}