using Cohort.GameRunner.Loading;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(99)] // before Gameloader
public class LobbyLoader : MonoBehaviour {
	private const string LOBBY_SCENE = "01-Lobby";
	
    private void Awake() {
	    SceneManager.sceneLoaded += OnSceneLoaded;
	    if (Network.Local.Client.InRoom) {
		    LoadLobby();
	    }
	    else {
		    Network.Local.Callbacks.onJoinedRoom += LoadLobby;
	    }
    }
    
    private void LoadLobby() {
	    LoadingManager.Instance[LoadPhase.Lobby, LoadType.LoadLobbyScene].Start();
	    SceneManager.LoadScene(LOBBY_SCENE, LoadSceneMode.Additive);
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { 
	    if (scene.name == LOBBY_SCENE) {
		    LoadingManager.Instance[LoadPhase.Lobby, LoadType.LoadLobbyScene].Finish();
	    }
    }
}
