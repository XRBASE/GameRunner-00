using System;
using System.Linq;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Minigames;
using Cohort.GameRunner.Players;
using Cohort.Patterns;

public class HighscoreTracker : Singleton<HighscoreTracker> {
	public PlayerScore Local {
		get { return _local; }
	}

	public Action<PlayerScore[]> onScoresUpdated;
	
	private int _session;
	
	private PlayerScore _local;
	private Dictionary<string, PlayerScore> _scores = new Dictionary<string, PlayerScore>();

	private void Start() {
		Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
		Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
		
		MinigameManager.Instance.onMinigameFinished += OnLearningFinished;
	}
	
	public void Initialize(ActivityDescription _activity, int session) {
		_session = session;
		_local = new PlayerScore(0, Player.Local.Name);
		
		if (Network.Local.Client.InRoom) {
			OnJoinedRoom();
		}
	}

	private void OnDestroy() {
		Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
		Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoomPropertiesChanged;
		
		MinigameManager.Instance.onMinigameFinished -= OnLearningFinished;
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
				if (kv_prop.Value == null) {
					ClearLocalScore();
				}
				else {
					_scores[uuid] = JsonUtility.FromJson<PlayerScore>((string)kv_prop.Value);
				
					if (uuid == Player.Local.UUID) {
						_local = _scores[uuid];
					}

					if (_scores[uuid].score == 0) {
						_scores.Remove(uuid);
					}
					
					changed = true;
				}
			}
		}

		if (changed) {
			if (_scores == null) {
				onScoresUpdated?.Invoke(Array.Empty<PlayerScore>());
			}
			else {
				onScoresUpdated?.Invoke(_scores.Values.OrderBy(s => s.score).Reverse().ToArray());
			}
		}
	}

	public PlayerScore[] GetScores()
	{
		return _scores.Values.OrderBy(s => s.score).Reverse().ToArray();
	}

	public void OnLearningFinished(Minigame.FinishCause cause, int score) {
		_local.score += score;
		
		if (_local.name == "warLott" || _local.name == "Itsa_Lott") {
			UpdateLocalPlayerScore(new PlayerScore(_local.score + 1, _local.name));
		}
		else {
			UpdateLocalPlayerScore(_local);
		}
	}

	public void ClearLocalScore() {
		_local.score = 0;
		_scores[Player.Local.UUID] = _local;
		
		UpdateLocalPlayerScore(_local);
	}

	private void UpdateLocalPlayerScore(PlayerScore score) {
		Hashtable changes = new Hashtable();
		changes.Add(GetPlayerSessionScoreKey(Player.Local.UUID), JsonUtility.ToJson(score));

		Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
	}
	
	public string GetPlayerSessionScoreKey(string playerUuid = "") {
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
