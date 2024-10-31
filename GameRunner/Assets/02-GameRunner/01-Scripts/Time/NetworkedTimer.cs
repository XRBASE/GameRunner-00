using System;
using System.Collections;
using Cohort.Networking.PhotonKeys;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class NetworkedTimer : MonoBehaviour {
    public Action onFinish;
    
    private bool _active;
    private float _duration;
    private float _expire;
    
    private bool _initialized;
    private bool _invoked;

    private void Start() {
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnPropertiesChanged;
        
        TimeManager.Instance.onRefTimeReset += OnTimeReset;
        
        _active = false;
        _invoked = false;
        _initialized = false;
        
        if (Network.Local.Client.InRoom) {
            OnJoinedRoom();
        }
    }

    private void OnDestroy() {
        Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropertiesChanged;
        
        TimeManager.Instance.onRefTimeReset -= OnTimeReset;
    }

    private void Update() {
        if (!_invoked && _expire > 0 && _expire <= TimeManager.Instance.RefTime) {
            _invoked = true;
            StartCoroutine(OnTimerFinished());
        }
    }

    private IEnumerator OnTimerFinished() {
        Hashtable changes = new Hashtable();
        changes.Add(GetStateKey(), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);

        //TODO_COHORT: replace fixedupdate by some other form of network update.
        yield return new WaitForFixedUpdate();
        onFinish?.Invoke();
    }

    public void StartTimer(float duration) {
        _duration = duration;
        _expire = TimeManager.Instance.RefTime + _duration;

        string key = GetStateKey();
        Hashtable expected = new Hashtable();
        expected.Add(key, false);
        
        Hashtable changes = new Hashtable();
        changes.Add(key, true);
        changes.Add(GetTimeKey(), _expire);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
    }

    public void StopTimer() {
        if (_initialized && !_active)
            return;

        Hashtable changes = new Hashtable();
        changes.Add(GetTimeKey(), -1);
        changes.Add(GetStateKey(), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void OnTimeReset() {
        if (_active) {
            _expire %= TimeManager.RESET_VALUE;
        }
    }

    private void OnJoinedRoom() {
        OnPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);

        if (!_initialized) {
            StopTimer();
        }
    }
    
    private void OnPropertiesChanged(Hashtable changes) {
        string key = GetTimeKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                StopTimer();
            }
            else {
                _expire = float.Parse(changes[key].ToString());
                _duration = _expire - TimeManager.Instance.RefTime;
            }
        }
        
        key = GetStateKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                _initialized = false;
                
                StopTimer();
            }
            else {
                _initialized = true;
                _active = (bool)changes[key];
                if (_active) {
                    _invoked = false;
                }
            }
        }
    }

    private string GetStateKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Time), Keys.Get(Keys.Time.TimerState));
    }

    private string GetTimeKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Time), Keys.Get(Keys.Time.TimerTime));
    }
}
