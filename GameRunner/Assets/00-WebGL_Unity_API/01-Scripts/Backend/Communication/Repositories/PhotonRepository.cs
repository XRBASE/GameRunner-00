using Cohort.Networking;
using Cohort.Config;

using System.Collections;
using System;

public class PhotonRepository 
{
    /// <summary>
    /// Connect to photon room with given id.
    /// </summary>
    /// <param name="sessionId">id(unique) of room to join.</param>
    /// <param name="onComplete">Callback for when room join call has been completed.</param>
    /// <param name="onFailure">Callback for server error whilst retrieving room data.</param>
    public IEnumerator ConnectToPhotonRoom(string sessionId, Action onComplete, Action<string> onFailure) {
        RavelWebRequest req = SessionRequest.GetSession(sessionId);
        yield return req.Send();

        SessionResponse session;
        RavelWebResponse res = new RavelWebResponse(req);
        if (res.Success && res.TryGetData(out session)) {
            yield return ConnectToPhotonRoom(session);
            onComplete?.Invoke();
        } else {
            onFailure?.Invoke(res.Error.FullMessage);
        }
    }

    private IEnumerator ConnectToPhotonRoom(SessionResponse session) {
        //prevent updates in data from causing errors while the application is not connected to photon
        DataServices.Assets.StopSlotUpdate();
        DataServices.Users.StopAutoUserUpdate();
            
        AppConfig.Config.OverrideAppID(session.photonAppId);
        Network.Local.UpdateAppId(session.photonAppId);
        
        yield return Network.Local.Callbacks.WaitForConnectedAndReady();
        if (!Network.Local.Client.InRoom && session.id != Network.Local.RoomManager.CurrentRoomId) {
            Network.Local.RoomManager.GotoRoom(session.id, false, false);
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
    private struct SessionResponse
    {
        /// <summary>
        /// Debug constructor for testing purposes, assigns users default roles, and gives response with given key
        /// </summary>
        /// <param name="photonRoomId">id/key for room</param>
        public SessionResponse(string roomName)
        {
            id = $"{roomName}_{DataServices.Users.Local.id}";

            throw new Exception("No runtime RoomDetailResponse init exists yet!");
        }
            
        //network id of photonroom
        public string id;
        //App id to which photon will be connected.
        public string photonAppId;
    }
}