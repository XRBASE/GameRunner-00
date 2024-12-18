using System.Collections.Generic;
using System.Linq;
using System;
using ExitGames.Client.Photon;
using UnityEngine;

using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Minigames;
using Cohort.GameRunner.Players;
using Cohort.Patterns;

[DefaultExecutionOrder(102)] // After playermanagement
public class ActivityLoader : Singleton<ActivityLoader> {
    public bool InActivity { get; private set; }
    public bool AllPlayersReady { get; private set; }

    public ActivityDescription Activity {
        get { return _description; }
    }

    public Action onActivityStart; 
    public Action onActivityStop; 

    [SerializeField] private int _session = -1;
    [SerializeField] private HighscoreTracker _score; 
    private ActivityDescription _description;

    private bool _sceneLoaded, _activityLoaded;

    private void Start() {
        Network.Local.Callbacks.onJoinedRoom += OnJoinRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnPropsChanged;

        if (Network.Local.Client.InRoom) {
            OnJoinRoom();
        }
        
        EnvironmentLoader.Instance.onEnvironmentLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        EnvironmentLoader.Instance.onEnvironmentLoaded -= OnSceneLoaded;
        
        Network.Local.Callbacks.onJoinedRoom -= OnJoinRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropsChanged;
    }
    
    private void StartActivity() {
        onActivityStart?.Invoke();
        
        MinigameManager.Instance.OnActivityStart(Activity.ScoreMultiplier);
    }
    
    public void StopActivity() {
        Hashtable props = new Hashtable();
        props[GetActivityDefKey()] = JsonUtility.ToJson(ActivityDescription.EMPTY);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(props);
        Debug.Log("Activity stop sent");
    }

    private void StopActivityLocal() {
        if (!InActivity)
            return;
        
        Debug.Log("Activity stop local");
        InActivity = false;
        AllPlayersReady = false;
        
        MinigameManager.Instance.OnActivityStop();
        
        onActivityStop?.Invoke();
        ClearPhotonRoomProperties();
    }

    private void OnSceneLoaded(string name) {
        if (!InActivity) {
            return;
        }
        
        if (_description == null) {
            Debug.LogError("Missing game description\n Please reload the activity!");
            return;
        }
        
        //TODO_COHORT: Assetbundles
        if (_description.AssetRef == name) {
            OnLocalPlayerReady();
        }
    }

    private void OnLocalPlayerReady() {
        Hashtable changes = new Hashtable();
        changes.Add(GetPlayerReadyKey(Player.Local.ActorNumber), true);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void OnPlayerReady() {
        if (AllPlayersReady)
            return;
        
        //TODO_COHORT: there should be a from of start key, so this is skipped when reconnecting.
        //If 3 people reconnect they all have to wait on eachother right now, but that shouldn't happen.
        string key;
        foreach (var kv_player in Network.Local.Client.CurrentRoom.Players) {
            if (kv_player.Value.IsInactive)
                continue;
            
            key = GetPlayerReadyKey(kv_player.Value.ActorNumber);
            if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) ||
                !(bool)Network.Local.Client.CurrentRoom.CustomProperties[key]) {
                return;
            }
        }

        AllPlayersReady = true;
        StartActivity();
    }
    
    private void OnJoinRoom() {
        OnPropsChanged(Network.Local.Client.CurrentRoom.CustomProperties);
    }
    
    private void OnPropsChanged(Hashtable changes) {
        string key;
        
        key = GetActivitySessionKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                _session = -1;
                return;
            }

            _session = (int)changes[key];
        }
        
        key = GetActivityDefKey();
        if (changes.ContainsKey(key)) {
            if (string.IsNullOrEmpty((string)changes[key])) {
                _description = ActivityDescription.EMPTY;
            }
            else {
                _description = JsonUtility.FromJson<ActivityDescription>((string)changes[key]);
            }

            if (_description.IsEmpty) {
                if (InActivity) {
                    StopActivityLocal();
                }
            }
            else {
                LoadActivityLocal();
            }
        }
        
        if (!AllPlayersReady) {
            key = GetPlayerReadyKey();
            foreach (var entry in changes) {
                if (entry.Key.ToString().StartsWith(key)) {
                    OnPlayerReady();
                    break;
                }
            }
        }
    }
    
    private void ClearPhotonRoomProperties() {
        Hashtable props = Network.Local.Client.CurrentRoom.CustomProperties;
        //clear old data
        List<object> keys = props.Keys.ToList();
        foreach (var key in keys) {
            if (key.ToString().StartsWith(HighscoreTracker.Instance.GetPlayerSessionScoreKey())) {
                continue;
            }
            
            //set all properties to null (this should clear them out)
            props[key] = null;
        }
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(props);
        Debug.Log("Room properties cleared");
    }

    public void LoadActivity(ActivityDescription description) {
        if (InActivity)
            return;
        
        Hashtable changes = new Hashtable();
        changes.Add(GetActivityDefKey(), JsonUtility.ToJson(description));
        changes.Add(GetActivitySessionKey(), _session + 1);
        changes.Add(GetSceneKey(), description.AssetRef);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void LoadActivityLocal() {
        InActivity = true;
        
        _score.Initialize(_description, _session);
    }

    private string GetActivityDefKey() {
        return Keys.Concatenate(Keys.Room.Activity, Keys.Activity.Definition);
    }
    
    private string GetActivitySessionKey() {
        return Keys.Concatenate(Keys.Room.Activity, Keys.Activity.Session);
    }

    private string GetPlayerReadyKey(int actorNumber = -1) {
        string key = Keys.Concatenate(Keys.Room.Activity, Keys.Activity.PlayerReady);
        if (actorNumber >= 0) {
            return Keys.Concatenate(key, actorNumber.ToString());
        }

        return key;
    }
    
    private string GetSceneKey() {
        return Keys.Room.Scene.ToString();
    }
}
