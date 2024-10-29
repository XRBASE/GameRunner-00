using System;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;

public class NetworkedTimer : MonoTimer {
    private float _expire;
    private bool _initialized;

    private void Start() {
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnPropertiesChanged;
        
        TimeManager.Instance.onRefTimeReset += OnTimeReset;
        
        if (Network.Local.Client.InRoom) {
            OnJoinedRoom();
        }
    }

    private void OnDestroy() {
        Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropertiesChanged;
        
        TimeManager.Instance.onRefTimeReset -= OnTimeReset;
    }

    public void StartTimer(float duration) {
        if (_timer.Active)
            return;
        
        _duration = duration;
        StartTimer();
    }

    public override void StartTimer() {
        if (_timer.Active)
            return;
        
        _expire = TimeManager.Instance.RefTime + _duration;

        string key = GetActiveKey();
        Hashtable changes = new Hashtable();
        changes.Add(key, true);

        Hashtable expected = new Hashtable();
        expected.Add(key, false);
        changes.Add(GetTimeKey(), _expire);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
    }

    public override void StopTimer() {
        if (!_timer.Active)
            return;

        Hashtable changes = new Hashtable();
        changes.Add(GetTimeKey(), -1);
        changes.Add(GetActiveKey(), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    public virtual void ResetTimer() {
        throw new NotImplementedException();
    }

    public virtual void FinishTimer() {
        throw new NotImplementedException();
    }

    private void OnTimeReset() {
        if (_timer.Active) {
            _expire %= TimeManager.RESET_VALUE;
            _timer.duration = _expire - TimeManager.Instance.RefTime;
        }
    }

    private void OnJoinedRoom() {
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
                
                StopTimer();
            }
            else {
                _initialized = true;
                
                if (_timer.Active != (bool)changes[key]) {
                    if (_timer.Active) {
                        base.StopTimer();
                    }
                    else {
                        base.StartTimer();
                    }
                }
            }
        }

        key = GetTimeKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                StopTimer();
            }
            else {
                _expire = float.Parse(changes[key].ToString());
                _timer.duration = _expire - TimeManager.Instance.RefTime;
            }
        }
    }

    private string GetActiveKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Time), Keys.Get(Keys.Time.TimerActive));
    }

    private string GetTimeKey() {
        return Keys.Concatenate(Keys.Get(Keys.Room.Time), Keys.Get(Keys.Time.TimerTime));
    }
}
