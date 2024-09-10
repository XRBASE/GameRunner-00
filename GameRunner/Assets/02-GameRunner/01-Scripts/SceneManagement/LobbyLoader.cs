using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyLoader : MonoBehaviour {
	private const string LOBBY_SCENE = "01-Lobby";
	
    private void Awake() {
	    if (Network.Local.Client.InRoom) {
		    LoadLobby();
	    }
	    else {
		    Network.Local.Callbacks.onJoinedRoom += LoadLobby;
	    }
    }
    
    private void LoadLobby() {
	    SceneManager.LoadSceneAsync(LOBBY_SCENE, LoadSceneMode.Additive);
    }
}
