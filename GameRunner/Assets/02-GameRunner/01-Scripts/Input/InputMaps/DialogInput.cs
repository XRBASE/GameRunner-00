using System;
using UnityEngine.InputSystem;

namespace Cohort.GameRunner.Input.Maps {
	public class DialogInput : InputMap {
		public Action onNext;
		public Action onEscape;
		
		public DialogInput(InputActionAsset actions) : base(actions, "Dialog") {
			_map["NextMessage"].started += OnNextAction;
		}

		public override void Dispose() {
			if (!DataServices.Login.UserLoggedIn)
				return;

			_map["NextMessage"].started -= OnNextAction;
			
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