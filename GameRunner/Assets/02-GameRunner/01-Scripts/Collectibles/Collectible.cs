using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Collectible : BaseInteractable {
	
	[SerializeField] private UnityEvent onReset;
	[SerializeField] private UnityEvent onCollectCinematic;
	[SerializeField] private UnityEvent onCollectDirect;

	public override bool CheckInRange() {
		if (!Value)
			return base.CheckInRange();
		else
			return false;
	}
	
	public override void OnInteract() {
		if (!Value)
			Activate();
	}

	protected override void ActivateLocal() {
		if (Initial) {
			onCollectDirect?.Invoke();
		}
		else {
			onCollectCinematic?.Invoke();
		}
	}

	protected override void DeactivateLocal() {
		onReset?.Invoke();
	}

	protected void OnTriggerEnter(Collider other) {
		if (other.transform == Player.Local.transform) {
			Activate();
		}
	}
}
