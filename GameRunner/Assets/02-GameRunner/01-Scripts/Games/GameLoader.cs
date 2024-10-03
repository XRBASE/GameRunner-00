using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Players;
using Cohort.Patterns;

using System.Collections.Generic;
using System.Linq;
using System;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine;

[DefaultExecutionOrder(100)] // After environment loader
public class GameLoader : Singleton<GameLoader>
{
    public bool InGame { get; private set; }
    public bool AllPlayersReady { get; private set; }

    public Action onActivityStart; 
    public Action onActivityStop; 

    [SerializeField] private int _session = -1;
    private GameDefinition _definition;

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
        Debug.LogError("All Players ready");
        onActivityStart?.Invoke();
    }
    
    public void StopGame() {
        if (!InGame)
            return;

        InGame = false;
        AllPlayersReady = false;
        
        onActivityStop?.Invoke();
        ClearPhotonRoomProperties();
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
        
        key = GetGameSessionKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                _session = -1;
                return;
            }

            _session = (int)changes[key];
        }
        
        key = GetGameDefKey();
        if (changes.ContainsKey(key)) {
            if (string.IsNullOrEmpty((string)changes[key])) {
                StopGameLocal();
                return;
            }

            _definition = JsonUtility.FromJson<GameDefinition>((string)changes[key]);
            LoadGameLocal();
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

    private void StopGameLocal() {
        if (!InGame)
            return;
        
        InGame = false;
        _definition = null;
    }

    public void LoadGame(GameDefinition definition) {
        if (InGame)
            return;
        
        Hashtable changes = new Hashtable();
        changes.Add(GetGameDefKey(), JsonUtility.ToJson(definition));
        changes.Add(GetGameSessionKey(), _session + 1);
        changes.Add(GetSceneKey(), definition.AssetRef);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void LoadGameLocal() {
        InGame = true;
    }

    private string GetGameDefKey() {
        return Keys.Concatenate(Keys.Room.Game, Keys.Game.Definition);
    }
    
    private string GetGameSessionKey() {
        return Keys.Concatenate(Keys.Room.Game, Keys.Game.Session);
    }

    private string GetPlayerReadyKey(int actorNumber = -1) {
        string key = Keys.Concatenate(Keys.Room.Game, Keys.Game.PlayerReady);
        if (actorNumber >= 0) {
            return Keys.Concatenate(key, actorNumber.ToString());
        }

        return key;
    }
    
    private string GetSceneKey() {
        return Keys.Room.Scene.ToString();
    }
}
