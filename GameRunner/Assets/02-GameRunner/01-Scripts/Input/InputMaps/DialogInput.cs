using System;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps {
	public class DialogInput : InputMap {
		public Action onNext;
		public Action onEscape;

		private InputAction _nextAction;
		//private InputAction _escapeAction;

		public DialogInput(InputActionAsset actions) : base(actions, "Dialog") {
			_nextAction = _map.FindAction("NextMessage");
			//_escapeAction = _map.FindAction("EscapeDialog");

			_nextAction.started += OnNextAction;
			//_escapeAction.started += OnEscapeAction;
		}

		public override void Dispose() {
			if (!DataServices.Login.UserLoggedIn)
				return;

			_nextAction.canceled -= OnNextAction;
			//_escapeAction.performed -= OnEscapeAction;

			//_escapeAction.Dispose();
			_nextAction.Dispose();
			onNext = null;
			onEscape = null;

			base.Dispose();
		}

		private void OnNextAction(InputAction.CallbackContext obj) {
			if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
				return;

			onNext?.Invoke();
		}

		private void OnEscapeAction(InputAction.CallbackContext obj) {
			if (InputManager.Instance.TypingInput.PlayerTyping || !DataServices.Login.UserLoggedIn)
				return;

			onEscape?.Invoke();
		}
	}
}