using System;
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

    
    private HashSet<int> actors;

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

        actors = new HashSet<int>();
        
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnRoompropsChanged;

        if (Network.Local.Client.InRoom) {
            OnJoinedRoom();
        }
    }

    private void OnDestroy() {
        Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
        Network.Local.Callbacks.onPlayerLeftRoom -= OnPlayerLeftRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoompropsChanged;
    }

    private void OnPlayerLeftRoom(Photon.Realtime.Player player) {
        if (actors.Contains(player.ActorNumber)) {
            RemovePlayer(player.ActorNumber);
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
                if (kv_change.Value == null)
                    continue;

                if ((bool)kv_change.Value) {
                    //add player local
                    if (TryGetActorFromKey(kv_change.Key.ToString(), out int actor)) {
                        if (PlayerManager.Instance.ActorNumberExists(actor)) 
                        {
                            actors.Add(actor);
                        }
                        else {
                            ClearPlayer(actor.ToString());
                        }
                    }
                }
                else {
                    //remove player local
                    if (TryGetActorFromKey(kv_change.Key.ToString(), out int actor)) {
                        if (actors.Contains(actor)) {
                            actors.Remove(actor);
                        }
                    }
                }
            }
        }

        //no changes escape
        _curCount = actors.Count;
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
                AddPlayer(Player.Local.ActorNumber);
                _containsLocal = true;
            }
        }
        else {
            if (_containsLocal) {
                RemovePlayer(Player.Local.ActorNumber);
                _containsLocal = false;
            }
        }
    }

    private void AddPlayer(int actor) {
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerKey(actor.ToString()), true);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    /// <summary>
    /// Removes player from the list of players in the area.
    /// </summary>
    /// <param name="actor">Actor number of player.</param>
    private void RemovePlayer(int actor) {
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerKey(actor.ToString()), false);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }
    
    /// <summary>
    /// Clears the player data. This does not explicitly remove the player from the list, but rather removes the entry of the player from the photondata.
    /// </summary>
    /// <param name="actor">Actor number of player.</param>
    private void ClearPlayer(string actor) {
        Debug.LogWarning($"GroupTrigger: Clear data of player {actor}!");
        
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerKey(actor), null);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }
    
    private string GetPlayerKey(string actor) {
        if (string.IsNullOrEmpty(actor)) {
            return Keys.Get(Keys.Room.Group);
        }
        else {
            return Keys.GetUUID(Keys.Room.Group, actor);
        }
    }

    private bool TryGetActorFromKey(string key, out int actor) {
        string[] keys = key.Split(Keys.SEPARATOR);
        if (keys.Length == 1) {
            actor = -1;
            return false;
        }
        else {
            if (int.TryParse(keys[^1], out actor)) {
                return true;
            }
            
            //clear any invalid keys
            ClearPlayer(key);
            return false;
        }
    }
    
#if UNITY_EDITOR
    public virtual void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif
}
