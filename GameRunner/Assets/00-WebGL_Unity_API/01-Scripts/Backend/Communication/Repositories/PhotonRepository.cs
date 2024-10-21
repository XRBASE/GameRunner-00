using Cohort.Networking;
using Cohort.Config;

using System.Collections;
using System;

public class PhotonRepository 
{
    /// <summary>
    /// Connect to photon room with given id.
    /// </summary>
    /// <param name="roomId">id(unique) of room to join.</param>
    /// <param name="onComplete">Callback for when room join call has been completed.</param>
    /// <param name="onFailure">Callback for server error whilst retrieving room data.</param>
    public IEnumerator ConnectToPhotonRoom(string roomId, Action onComplete, Action<string> onFailure) {
        RavelWebRequest req = RoomRequest.GetRoom(roomId);
        yield return req.Send();

        RoomDetailResponse roomDetails;
        RavelWebResponse res = new RavelWebResponse(req);
        if (res.Success && res.TryGetData(out roomDetails)) {
            yield return ConnectToPhotonRoom(roomDetails);
            onComplete?.Invoke();
        } else {
            onFailure?.Invoke(res.Error.FullMessage);
        }
    }

    private IEnumerator ConnectToPhotonRoom(RoomDetailResponse room) {
        //prevent updates in data from causing errors while the application is not connected to photon
        DataServices.Assets.StopSlotUpdate();
        DataServices.Users.StopAutoUserUpdate();
            
        AppConfig.Config.OverrideAppID(room.photonAppId);
        Network.Local.UpdateAppId(room.photonAppId);
        
        yield return Network.Local.Callbacks.WaitForConnectedAndReady();
        if (!Network.Local.Client.InRoom && room.id != Network.Local.RoomManager.CurrentRoomId) {
            Network.Local.RoomManager.GotoRoom(room.id, false, false);
        }
        
        DataServices.Assets.StartSlotUpdate();
        DataServices.Users.StartAutoUserUpdate();
    }

    [Serializable]
    public struct RoomIDResponse
    {
        public string roomId;
    }
    
    /// <summary>
    /// Response that is send back from the server when a user enters a specific space. 
    /// </summary>
    [Serializable]
    private struct RoomDetailResponse
    {
        /// <summary>
        /// Debug constructor for testing purposes, assigns users default roles, and gives response with given key
        /// </summary>
        /// <param name="photonRoomId">id/key for room</param>
        public RoomDetailResponse(string roomName)
        {
            id = $"{roomName}_{DataServices.Users.Local.id}";

            throw new Exception("No runtime RoomDetailResponse init exists yet!");
        }
            
        //network id of photonroom
        public string id;
        //App id to which photon will be connected.
        public string photonAppId;
        
        //(unused but available) Servername of photon room
        //public string photonId; 
        //public string spaceEntityId;
    }
}