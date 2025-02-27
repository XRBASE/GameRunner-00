using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Collectible : BaseInteractable {
	
	[SerializeField, Tooltip("Is fired when resetting the collectible, so it can be grabbed again.")]
	private UnityEvent onReset;
	[SerializeField, Tooltip("Is fired when the item is grabbed by any player.")]
	private UnityEvent onCollectCinematic;
	[SerializeField, Tooltip("Is fired when joining the room, to preset the value of the collectible.")]
	private UnityEvent onCollectDirect;
	[SerializeField, Tooltip("Is fired along with the cinematic option, if the item was collected by the local player.")]
	private UnityEvent onCollectLocal;

	[SerializeField] private int _score;

	private bool _localTrigger = false;

	protected override bool CheckInteractRange() {
		if (!Value)
			return base.CheckInteractRange();
		else {
			InInteractRange = false;
			return InInteractRange;
		}
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

		if (_localTrigger) {
			HighscoreTracker.Instance.AddPoints(_score);
			onCollectLocal?.Invoke();
		}

		_localTrigger = false;
	}

	protected override void DeactivateLocal() {
		onReset?.Invoke();
	}

	protected void OnTriggerEnter(Collider other) {
		if (other.transform == Player.Local.transform) {
			_localTrigger = true;
			Activate();
		}
	}
}
