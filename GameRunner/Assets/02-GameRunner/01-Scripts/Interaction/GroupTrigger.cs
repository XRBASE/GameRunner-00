using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine.Events;
using UnityEngine;

using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Players;

public class GroupTrigger : MonoBehaviour {
    private const string COUNT_PLACEHOLDER = "[COUNT]";
    private const string MIN_PLACEHOLDER = "[MIN]";
    private const string MAX_PLACEHOLDER = "[MAX]";
    
    [SerializeField, Tooltip("Minimum amount of players for trigger to be activated")] 
    private int _minimum = -1;
    [SerializeField, Tooltip("Maximum amount of players for trigger to be activated, leave -1 if there is no maximum")] 
    private int _maximum = -1;
    [SerializeField] private float _radius = 5;

    [SerializeField] private UnityEvent onActivateCinematic;
    [SerializeField] private UnityEvent onActivateDirect;
    [SerializeField] private UnityEvent onDeactivateCinematic; 
    [SerializeField] private UnityEvent onDeactivateDirect;
    
    [SerializeField] private string countStringFormat = "[COUNT] of [MIN]";
    [SerializeField] private UnityEvent<string> onCountChanged;

    private List<string> uuids;

    private bool _value;
    private bool _containsLocal;
    private int _curCount;

    private void Start() {
        if (_maximum < 0) {
            _maximum = int.MaxValue;
        }

        if (countStringFormat.Contains(MIN_PLACEHOLDER)) {
            countStringFormat = countStringFormat.Replace(MIN_PLACEHOLDER, _minimum.ToString());
        }
        if (countStringFormat.Contains(MAX_PLACEHOLDER)) {
            countStringFormat = countStringFormat.Replace(MAX_PLACEHOLDER, _maximum.ToString());
        }

        string invokeString;
        if (countStringFormat.Contains(COUNT_PLACEHOLDER)) {
            invokeString = countStringFormat.Replace(COUNT_PLACEHOLDER, _curCount.ToString());
        }
        else {
            invokeString = countStringFormat;
        }
        onCountChanged?.Invoke(invokeString);

        uuids = new List<string>();
        
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnRoompropsChanged;

        if (Network.Local.Client.InRoom) {
            OnJoinedRoom();
        }
    }

    private void OnJoinedRoom() {
        OnRoompropsChanged(Network.Local.Client.CurrentRoom.CustomProperties, true);
    }

    private void OnRoompropsChanged(Hashtable changes) {
        OnRoompropsChanged(changes, false);
    }

    private void OnRoompropsChanged(Hashtable changes, bool initial) {
        int prevCount = _curCount;
        
        //Count players
        string baseKey = GetPlayerKey("");
        foreach (var kv_change in changes) {
            if (kv_change.Key.ToString().StartsWith(baseKey)) {
                if ((bool)kv_change.Value) {
                    //add player local
                    if (TryGetUUIDFromKey(kv_change.Key.ToString(), out string uuid)) {
                        uuids.Add(uuid);
                    }
                }
                else {
                    //remove player local
                    if (TryGetUUIDFromKey(kv_change.Key.ToString(), out string uuid)) {
                        uuids.Remove(uuid);
                    }
                }
            }
        }

        //no changes escape
        _curCount = uuids.Count;
        if (_curCount == prevCount)
            return;
        string invokeString;
        //invoke count changed
        if (countStringFormat.Contains(COUNT_PLACEHOLDER)) {
            invokeString = countStringFormat.Replace(COUNT_PLACEHOLDER, _curCount.ToString());
        }
        else {
            invokeString = countStringFormat;
        }
        onCountChanged?.Invoke(invokeString);
        
        //check if in or outside of limits.
        if (!_value) {
            if (_curCount >= _minimum && _curCount <= _maximum) {
                if (prevCount < _minimum || prevCount > _maximum) {
                    Activate(initial);
                }
            }
        } else {
            if (_curCount < _minimum || _curCount > _maximum) {
                if (prevCount >= _minimum && prevCount <= _maximum) {
                    Deactivate(initial);
                }
            }
        }
    }

    private void Activate(bool initial) {
        _value = true;
        
        if (initial)
            onActivateDirect?.Invoke();
        else 
            onActivateCinematic?.Invoke();
    }

    private void Deactivate(bool initial) {
        _value = false;
        
        if (initial)
            onDeactivateDirect?.Invoke();
        else 
            onDeactivateCinematic?.Invoke();
    }

    private void Update() {
        if ((Player.Local.transform.position - transform.position).magnitude <= _radius) {
            if (!_containsLocal) {
#if UNITY_EDITOR
                AddPlayer(Player.Local.UUID + "-editor");
#else
                AddPlayer(Player.Local.UUID);                
#endif
                
                _containsLocal = true;
            }
        }
        else {
            if (_containsLocal) {
#if UNITY_EDITOR
                RemovePlayer(Player.Local.UUID + "-editor");
#else
                RemovePlayer(Player.Local.UUID);
#endif
                _containsLocal = false;
            }
        }
    }

    private void AddPlayer(string uuid) {
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerKey(uuid), true);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void RemovePlayer(string uuid) {
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerKey(uuid), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }
    
    private string GetPlayerKey(string uuid) {
        if (string.IsNullOrEmpty(uuid)) {
            return Keys.Get(Keys.Room.Group);
        }
        else {
            return Keys.GetUUID(Keys.Room.Group, uuid);
        }
    }

    private bool TryGetUUIDFromKey(string key, out string uuid) {
        uuid = key.Split(Keys.SEPARATOR)[^1];
        if (uuid == Keys.Get(Keys.Room.Group)) {
            uuid = "";
            return false;
        }
        else {
            return true;
        }
    }
    
#if UNITY_EDITOR
    public virtual void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif
}
