using System;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps {
    /// <summary>
    /// Dev feature input mappings.
    /// </summary>
    public class DevInput : InputMap {
        private const string TOGGLE_CONSOLE = "ToggleDebugConsole";

        public Action toggleConsole;
        private InputAction _toggleConsoleAction;

        public DevInput(InputActionAsset actions) : base(actions, "Dev") {
            _map[TOGGLE_CONSOLE].started += ToggleConsole;
        }

        public override void Dispose() {
            _map[TOGGLE_CONSOLE].started -= ToggleConsole;
            _map[TOGGLE_CONSOLE].Dispose();

            toggleConsole = null;
        }

        private void ToggleConsole(InputAction.CallbackContext context) {
            if (!DataServices.Login.UserLoggedIn)
                return;

            toggleConsole?.Invoke();
        }
    }
}