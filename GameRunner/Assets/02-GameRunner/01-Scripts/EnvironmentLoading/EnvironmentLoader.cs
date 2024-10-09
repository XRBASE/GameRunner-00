using System;
using Cohort.GameRunner.Loading;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(99)] // before Gameloader
public class EnvironmentLoader : Singleton<EnvironmentLoader> {
	private const string LOBBY_SCENE = "01-Lobby";

	public Action onEnvironmentLoaded;

	private bool _initial = true;
	private string _activeScene;
	
    protected override void Awake() {
	    base.Awake();
	    SceneManager.sceneLoaded += OnSceneLoaded;
	    
	    Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
	    Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
	    
	    LoadingManager.Instance[LoadPhase.Lobby, LoadType.LoadLobbyScene].Start();
	    _initial = true;
	    
	    if (Network.Local.Client.InRoom) {
		    OnJoinedRoom();
	    }
    }

    private void OnDestroy() {
	    SceneManager.sceneLoaded -= OnSceneLoaded;
	    
	    Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
	    Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoomPropertiesChanged;
    }

    private void OnJoinedRoom() {
	    OnRoomPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);
	    
	    if (string.IsNullOrEmpty(_activeScene)) {
		    LoadScene(LOBBY_SCENE);
	    }
    }
    
    private void OnRoomPropertiesChanged(Hashtable changes) {
	    string key = GetSceneKey();
	    if (changes.ContainsKey(key)) {
		    string scene;
		    if (changes[key] == null) {
			    scene = LOBBY_SCENE;
		    }
		    else {
			    scene = (string)changes[key];
		    }
		    
		    LoadSceneLocal(scene);
	    }
    }

    private void LoadScene(string sceneName) {
	    Hashtable changes = new Hashtable();
	    changes.Add(GetSceneKey(), sceneName);

	    Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void LoadSceneLocal(string sceneName) {
	    //TODO_COHORT: assetbundles
	    if (!string.IsNullOrEmpty(_activeScene)) {
		    SceneManager.UnloadSceneAsync(_activeScene);
	    }

	    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
	    _activeScene = sceneName;
	    onEnvironmentLoaded?.Invoke();
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
	    if (_initial) {
		    LoadingManager.Instance[LoadPhase.Lobby, LoadType.LoadLobbyScene].Finish();
		    _initial = false;
	    }
    }

    private string GetSceneKey() {
	    return Keys.Room.Scene.ToString();
    }
}
