using Cohort.GameRunner.Input;
using UnityEngine;

namespace Cohort.GameRunner.Interaction {
	[DefaultExecutionOrder(2)] //After cursor raycaster 
	public class CursorInteraction : MonoBehaviour {
		private CursorRayCaster _raycaster;

		private void Start() {
			_raycaster = InputManager.Instance.Raycaster;
			InputManager.Instance.InteractInput.onClickInteract += OnInteract;
		}
		
		void OnInteract() {
			BaseInteractable i = _raycaster.CurHit.collider.GetComponent<BaseInteractable>();
			if (i == null) {
				//TODO_COHORT: really wanna do this?
				//does it even work?
				_raycaster.HitState = CursorRayCaster.RCHitState.Navigate;
				return;
			}

			if (i.CheckInRange()) {
				i.OnInteract();
			}
		}
	}
}