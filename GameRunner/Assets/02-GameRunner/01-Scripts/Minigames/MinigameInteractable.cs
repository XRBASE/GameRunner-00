using System;
using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MinigameInteractable : Interactable {
	public bool HasMinigame { get; private set; }
	
	[SerializeField] private GameObject _inUseIndicator;
	[SerializeField] private ObjIndicator _minigameIndicator;

	private int _actor = -1;
	private int _minigameIndex = -1;

	public Action<int, MinigameInteractable> onMinigameStart;

	protected override void Start() {
		base.Start();
		
		Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
	}

	protected override void OnDestroy() {
		base.OnDestroy();
		
		Network.Local.Callbacks.onPlayerLeftRoom -= OnPlayerLeftRoom;
	}

	private void OnPlayerLeftRoom(Photon.Realtime.Player obj) {
		if (_actor == obj.ActorNumber) {
			_actor = -1;
			Deactivate();
		}
	}
	
	protected override void OnJoinedRoom() {
		base.OnJoinedRoom();
		
		if (Value && !PlayerManager.Instance.ActorNumberExists(_actor)) {
			_actor = -1;
			Deactivate();
		}
	}

	protected override void OnPropertiesChanged(Hashtable changes) {
		string key = GetMiniGameIdKey();
		if (changes.ContainsKey(key)) {
			if (changes[key] == null) {
				_minigameIndex = -1;
			}
			else {
				_minigameIndex = (int)changes[key];
			}

			HasMinigame = _minigameIndex >= 0;
			_minigameIndicator.SetActive(HasMinigame);
		}
		
		key = GetMiniGamePlayerKey();
		if (changes.ContainsKey(key)) {
			if (changes[key] == null) {
				_actor = -1;
			}
			else {
				_actor = (int)changes[key];
			}
		}
		
		base.OnPropertiesChanged(changes);
	}

	public override void OnInteract() {
		if (!HasMinigame)
			return;
		
		if (Value) {
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
		
		base.Deactivate(changes, expected);
	}

	protected override void ActivateLocal() {
		_inUseIndicator.SetActive(true);
		
		_minigameIndicator.SetActive(false);
		
		if (_actor == Player.Local.ActorNumber) {
			onMinigameStart?.Invoke(_minigameIndex, this);
		}
	}

	protected override void DeactivateLocal() {
		_inUseIndicator.SetActive(false);
	}
	
	public void ActivateMinigame(int minigameIndex) {
		Hashtable changes = new Hashtable();
		changes.Add(GetMiniGameIdKey(), minigameIndex);

		Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
	}

	public void DeactivateMinigame() {
		Hashtable changes = new Hashtable();
		changes.Add(GetMiniGameIdKey(), -1);

		Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
	}
	
	private string GetMiniGamePlayerKey() {
		return Keys.Concatenate(
			Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.Actor), 
			Identifier.ToString());
	}

	private string GetMiniGameIdKey() {
		return Keys.Concatenate(
			Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.Index), 
			Identifier.ToString());
	}
}
