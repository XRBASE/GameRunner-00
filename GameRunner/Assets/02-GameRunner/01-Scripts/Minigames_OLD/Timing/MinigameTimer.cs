using Cohort.Networking.PhotonKeys;

using Random = UnityEngine.Random;
using ExitGames.Client.Photon;
using System;
using UnityEngine;

public class MinigameTimer : MonoBehaviour {
    private const float T_MAX = 20f;
    private const float T_MIN = 10f;
    
    public Action onTimerFinished;
    
    private float _expire;
    private bool _timerActive;
    private bool _initialized;

    public void Start() {
        Network.Local.Callbacks.onJoinedRoom += OnJoinRoom;
        if (Network.Local.Client.InRoom) {
            OnJoinRoom();
        }
        
        Network.Local.Callbacks.onRoomPropertiesChanged += OnPropertiesChanged;
        TimeManager.Instance.onRefTimeReset += TimerReset;
    }
    
    private void OnDestroy() {
        Network.Local.Callbacks.onJoinedRoom -= OnJoinRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropertiesChanged;

        TimeManager.Instance.onRefTimeReset -= TimerReset;
    }

    public void StartTimer() {
        if (!_timerActive)
            return;
        
        float duration = Random.Range(T_MIN, T_MAX);

        float expire = TimeManager.Instance.RefTime + duration;

        string key = GetActiveKey();
        Hashtable changes = new Hashtable();
        changes.Add(key, false);
        
        Hashtable expected = new Hashtable();
        changes.Add(key, true);
        changes.Add(GetTimeKey(), expire);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
    }

    private void TimerReset() {
        if (_timerActive) {
            _expire %= TimeManager.RESET_VALUE;
        }
    }

    public void StopTimer() {
        Hashtable changes = new Hashtable();
        changes.Add(GetTimeKey(), -1);
        changes.Add(GetActiveKey(), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void OnTimerFinished() {
        onTimerFinished?.Invoke();
        
        StopTimer();
    }

    private void Update() {
        if (TimeManager.Instance.RefTime > _expire) {
            //DoSomething
            OnTimerFinished();
        }
    }

    private void OnJoinRoom() {
        OnPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);
        
        if (!_initialized) {
            StopTimer();
        }
    }
    
    private void OnPropertiesChanged(Hashtable changes) {
        string key = GetActiveKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                _initialized = false;
                _timerActive = false;
                StopTimer();
                return;
            }
            else {
                _timerActive = (bool)changes[key];
                _initialized = true;
            }
        }

        key = GetTimeKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                StopTimer();
            }
            else {
                _expire = float.Parse(changes.ToString());
                if (_timerActive) {
                    Debug.LogError($"Started minigame timer for {_expire - TimeManager.Instance.RefTime} seconds");
                }
            }
        }
    }
    
    private string GetActiveKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Minigame), Keys.Get(Keys.Minigame.TimerActive));
    }

    private string GetTimeKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Minigame), Keys.Get(Keys.Minigame.TimerTime));
    }
}
