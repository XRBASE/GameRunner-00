using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : Interactable {
	[SerializeField] private GameObject _inUseIndicator;

	private int _actor = -1;

	protected override void Start() {
		base.Start();
		
		Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
	}

	private void OnPlayerLeftRoom(Photon.Realtime.Player obj) {
		if (_actor == obj.ActorNumber) {
			_actor = -1;
			Deactivate();
		}
	}
	
	protected override void OnJoinedRoom() {
		base.OnJoinedRoom();
		
		if (Active && !PlayerManager.Instance.ActorNumberExists(_actor)) {
			_actor = -1;
			Deactivate();
		}
	}

	protected override void OnPropertiesChanged(Hashtable changes) {
		base.OnPropertiesChanged(changes);
		
		string key = GetMiniGamePlayerKey();
		if (changes.ContainsKey(key)) {
			if (changes[key] == null) {
				_actor = -1;
			}
			else {
				_actor = (int)changes[key];
			}
		}
	}

	public override void OnInteract() {
		if (Active) {
			Deactivate();
		}
		else {
			Activate();
		}
	}

	protected override void Activate(Hashtable changes, Hashtable expected = null) {
		if (changes == null) {
			changes = new Hashtable();
		}
		changes.Add(GetMiniGamePlayerKey(), Player.Local.ActorNumber);
		
		if (expected == null) {
			expected = new Hashtable();
		}
		expected.Add(GetInteractableKey(), false);
		
		base.Activate(changes, expected);
	}

	protected  override void Deactivate(Hashtable changes, Hashtable expected = null) {
		if (changes == null) {
			changes = new Hashtable();
		}
		changes.Add(GetMiniGamePlayerKey(), null);
		
		//if there is an actor, this item can only be deactivated by that actor
		if (_actor != -1) {
			if (expected == null) {
				expected = new Hashtable();
			}
			expected.Add(GetMiniGamePlayerKey(), Player.Local.ActorNumber);
		}
		
		//TODO_COHORT: prevent being called by user that's not in the minigame.
		
		base.Deactivate(changes, expected);
	}

	protected override void OnActivate() {
		_inUseIndicator.SetActive(true);
	}

	protected override void OnDeactivate() {
		_inUseIndicator.SetActive(false);
	}

	private string GetMiniGamePlayerKey() {
		return Keys.Concatenate(
			Keys.Concatenate(Keys.Room.Game, Keys.Game.Actor), 
			Identifier.ToString());
	}
}
