using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Players;
using Cohort.Patterns;

using System.Collections.Generic;
using System.Linq;
using System;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine;

[DefaultExecutionOrder(102)] // After playermanagement
public class ActivityLoader : Singleton<ActivityLoader>
{
    public bool InGame { get; private set; }
    public bool AllPlayersReady { get; private set; }

    public Action onActivityStart; 
    public Action onActivityStop; 

    [SerializeField] private int _session = -1;
    [SerializeField] private HighscoreTracker _score; 
    private ActivityDefinition _definition;

    private void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        Network.Local.Callbacks.onJoinedRoom += OnJoinRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnPropsChanged;

        if (Network.Local.Client.InRoom) {
            OnJoinRoom();
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        Network.Local.Callbacks.onJoinedRoom -= OnJoinRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropsChanged;
    }
    
    private void StartActivity() {
        onActivityStart?.Invoke();
        //TODO_COHORT: learning activity start
        LearningManager.Instance.OnActivityStart(_definition.ScoreMultiplier);
    }
    
    public void StopActivity() {
        if (!InGame)
            return;

        InGame = false;
        AllPlayersReady = false;
        
        onActivityStop?.Invoke();
        
        //TODO_COHORT: learning activity start
        LearningManager.Instance.OnActivityStop();
        
        
        ClearPhotonRoomProperties();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        //TODO_COHORT: Assetbundles
        if (_definition != null && scene.name == _definition.AssetRef) {
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
        string key = GetPlayerReadyKey();
        foreach (var entry in changes) {
            if (entry.Key.ToString().StartsWith(key)) {
                OnPlayerReady();
                break;
            }
        }
        
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
                StopActivityLocal();
                return;
            }

            _definition = JsonUtility.FromJson<ActivityDefinition>((string)changes[key]);
            LoadActivityLocal();
        }
    }
    
    private void ClearPhotonRoomProperties() {
        Hashtable props = Network.Local.Client.CurrentRoom.CustomProperties;
        //clear old data
        List<object> keys = props.Keys.ToList();
        foreach (var key in keys) {
            //set all properties to null (this should clear them out)
            props[key] = null;
        }

        Network.Local.Client.CurrentRoom.SetCustomProperties(props);
        Debug.Log("Room properties cleared");
    }

    private void StopActivityLocal() {
        if (!InGame)
            return;
        
        InGame = false;
        _definition = null;
    }

    public void LoadActivity(ActivityDefinition definition) {
        if (InGame)
            return;
        
        Hashtable changes = new Hashtable();
        changes.Add(GetActivityDefKey(), JsonUtility.ToJson(definition));
        changes.Add(GetActivitySessionKey(), _session + 1);
        changes.Add(GetSceneKey(), definition.AssetRef);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void LoadActivityLocal() {
        InGame = true;
        
        _score.Initialize(_definition, _session);
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
