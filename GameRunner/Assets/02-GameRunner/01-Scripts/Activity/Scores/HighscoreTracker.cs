using System;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using ExitGames.Client.Photon;
using UnityEngine;

public class HighscoreTracker : Singleton<HighscoreTracker> {
	public Action<PlayerScore[]> onScoresUpdated;
	
	private int _session;
	private int _multiplier = 100;
	
	private PlayerScore _local;
	private Dictionary<string, PlayerScore> _scores = new Dictionary<string, PlayerScore>();
	
	public void Initialize(ActivityDefinition _activity, int session) {
		_session = session;
		_multiplier = _activity.ScoreMultiplier;
		_local = new PlayerScore(0, Player.Local.Name);
		
		Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
		Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
		MinigameManager.Instance.onMinigameFinished += OnMinigameFinished;
		
		if (Network.Local.Client.InRoom) {
			OnJoinedRoom();
		}
	}

	private void OnJoinedRoom() {
		OnRoomPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);
	}
	
	private void OnRoomPropertiesChanged(Hashtable changes) {
		string key = GetPlayerSessionScoreKey();

		bool changed = false;
		foreach (var kv_prop in changes) {
			if (kv_prop.Key.ToString().StartsWith(key)) {
				string uuid = GetPlayerUuid(kv_prop.Key.ToString());
				_scores[uuid] = JsonUtility.FromJson<PlayerScore>((string)kv_prop.Value);
				
				if (uuid == Player.Local.UUID) {
					_local = _scores[uuid];
				}
				changed = true;
			}
		}

		if (changed) {
			//TODO: nullpointer
			onScoresUpdated?.Invoke(_scores.Values.OrderBy(s => s.score).Reverse().ToArray());
		}
	}

	public void OnMinigameFinished(float dec) {
		int mwap = Mathf.RoundToInt(dec * _multiplier);
		Debug.LogError($"({_local.name}) {_local.score} + {mwap} -> {_local.score + mwap} ({_multiplier})");
		_local.score += Mathf.RoundToInt(dec * _multiplier);
		
		UpdateLocalPlayerScore(_local);
	}

	private void UpdateLocalPlayerScore(PlayerScore score) {
		Hashtable changes = new Hashtable();
		changes.Add(GetPlayerSessionScoreKey(Player.Local.UUID), JsonUtility.ToJson(score));

		Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
	}
	
	private string GetPlayerSessionScoreKey(string playerUuid = "") {
		string key = Keys.Concatenate(Keys.Concatenate(Keys.Room.Activity, Keys.Activity.Score), _session.ToString());
		if (!string.IsNullOrEmpty(playerUuid)) {
			key = Keys.Concatenate(key, playerUuid);
		}

		return key;
	}

	private string GetPlayerUuid(string playerSessionKey) {
		return playerSessionKey.Split(Keys.SEPARATOR).Last();
	}

	[Serializable]
	public class PlayerScore {
		public string name;
		public int score;

		public PlayerScore(int score, string name) {
			this.name = name;
			this.score = score;
		}
	}
}
