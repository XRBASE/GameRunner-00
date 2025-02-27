using System;
using Cohort.GameRunner.Loading;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(101)] // Before ActivityLoader
public class EnvironmentLoader : Singleton<EnvironmentLoader> {
	
	public string CurrentScene {
		get { return _activeScene; }
	}

	public Action<string> onEnvironmentLoaded;

	private bool _initial = true;
	private string _activeScene;
	
    protected void Start() {
	    SceneManager.sceneLoaded += OnSceneLoaded;
	    
	    Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
	    Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
	    
	    LoadingManager.Instance[LoadPhase.Scene, LoadType.LoadEnvironmentScene].Start();
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
    }
    
    private void OnRoomPropertiesChanged(Hashtable changes) {
	    string key = GetSceneKey();
	    if (changes.ContainsKey(key)) {
		    string scene;
		    if (changes[key] == null) {
			    scene = "";
		    }
		    else {
			    scene = (string)changes[key];
			    LoadSceneLocal(scene);
		    }
	    }
    }

    public void LoadScene(string sceneName) {
	    Hashtable changes = new Hashtable();
	    changes.Add(GetSceneKey(), sceneName);

	    Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void LoadSceneLocal(string sceneName) {
	    if (sceneName == _activeScene) {
		    return;
	    }

	    if (_initial) {
		    LoadingManager.Instance[LoadPhase.Scene, LoadType.LoadEnvironmentScene].Increment("Loading initial scene");
	    }
	    
	    //TODO_COHORT: assetbundles
	    if (!string.IsNullOrEmpty(_activeScene)) {
		    SceneManager.UnloadSceneAsync(_activeScene);
	    }

	    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
	    _activeScene = sceneName;
	    
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
	    if (_initial) {
		    LoadingManager.Instance[LoadPhase.Scene, LoadType.LoadEnvironmentScene].Finish();
		    _initial = false;
	    }
	    
	    if (scene.name == _activeScene) {
		    onEnvironmentLoaded?.Invoke(_activeScene);
	    }
    }

    private string GetSceneKey() {
	    return Keys.Room.Scene.ToString();
    }
}
