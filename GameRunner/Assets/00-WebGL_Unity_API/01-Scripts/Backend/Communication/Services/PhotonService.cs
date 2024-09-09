using System;
using UnityEngine;

public class PhotonService : MonoBehaviour {
    private PhotonRepository _repo;

    private void Awake() {
        _repo = new PhotonRepository();
    }
    
    /// <summary>
    /// Connect to photon room with given id.
    /// </summary>
    /// <param name="roomId">id(unique) of room to join.</param>
    /// <param name="onComplete">Callback for when room join call has been completed.</param>
    /// <param name="onFailure">Callback for server error whilst retrieving room data.</param>
    public void ConnectToPhotonRoom(string roomId, Action onComplete, Action<string> onFailure) {
        StartCoroutine(_repo.ConnectToPhotonRoom(roomId, onComplete, onFailure));
    }
}
