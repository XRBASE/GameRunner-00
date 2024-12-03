using Cohort.GameRunner.Input;
using Cohort.GameRunner.Players;
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
			Interactable i = _raycaster.CurHit.collider.GetComponent<Interactable>();
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