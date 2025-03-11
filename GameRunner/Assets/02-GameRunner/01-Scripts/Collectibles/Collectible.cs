using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.GameRunner.Score;
using Cohort.Networking.PhotonKeys;
using UnityEngine;
using UnityEngine.Events;

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
	private bool _useIds;

	protected override void Start() {
		_useIds = _networked;
		_networked = true;
		
		base.Start();
	}

	protected override bool CheckInteractRange() {
		if (!Value)
			return base.CheckInteractRange();
		else {
			InInteractRange = false;
			return InInteractRange;
		}
	}
	
	public override bool CheckViewRange() {
		if (!Value)
			return base.CheckViewRange();
		else {
			InViewRange = false;
			return InViewRange;
		}
	}
	
	public override void OnInteract() {
		if (!Value) {
			_localTrigger = true;
			Activate();
		}
	}

	protected override void ActivateLocal() {
		base.ActivateLocal();
		
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
		base.DeactivateLocal();
		
		onReset?.Invoke();
	}

	protected void OnTriggerEnter(Collider other) {
		if (other.transform == Player.Local.transform) {
			_localTrigger = true;
			Activate();
		}
	}
	
	protected override string GetInteractableKey() {
		if (_useIds) {
			return Keys.Concatenate(Keys.GetUUID(Keys.Room.Interactable, Identifier.ToString()), Player.Local.UUID);
		}
		else {
			return base.GetInteractableKey();
		}
	}
}
