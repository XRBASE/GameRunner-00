using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

//no partial, but namespace
namespace Cohort.Ravel.PhotonNetworking.Callbacks
{
    //TODO: we might want to make this class partial over multiple files, so searching becomes easier, while usage retains just one reference?
    public class RealtimeCallbackHandle : IInRoomCallbacks, IMatchmakingCallbacks, IConnectionCallbacks
    {
        public bool ConnectedAndReady {
            get { return _client.IsConnectedAndReady; }
        }


        public Action<Hashtable> onRoomPropertiesChanged;
        public Action<Player, Hashtable> onPlayerPropertiesChanged; 
        
        public Action<Player> onPlayerEnteredRoom; 
        public Action<Player> onPlayerLeftRoom; 
        
        public Action onRoomCreated;
        public Action onRoomCreateFailed;
        public Action onJoinedRoom;
        public Action onJoinRoomFailed;

        //network update (data will be sent and recieved based on this call).
        public Action onService;
        
        /// <summary>
        /// This callback is called when the player has disconnected from the network (or is connecting to the next room),
        /// but has completely left the previous room. 
        /// </summary>
        public Action onLeftRoom;
        
        /// <summary>
        /// DOESNT SEEM TO WORK IN WEBGL TODO: see if working version is possible?
        /// This callback is called when the player is leaving the room, but has not done so yet, so the connection with photon is still active.
        /// </summary>
        public Action onLeavingRoom;

        public Action onConnect;
        public Action<DisconnectCause> onDisconnect;
        
        /// <summary>
        /// Called when reconnecting failed multiple times (amount stored in NetworkConfig), system will not try to reconnect after this
        /// </summary>
        public Action<DisconnectCause> onReconnectFailed;

        /// <summary>
        /// This is a one time callback, that auto clears when the system is connected and ready,
        /// for repeated use use onConnected, which fires when the user is connected to the master client.
        /// </summary>
        public Action onConnectedAndReady;
        
        private PhotonClient _client;
        private Network _network;
        
        public RealtimeCallbackHandle(Network network)
        {
            _network = network;
            _client = _network.Client;
            _client.AddCallbackTarget(this);
        }

        public void Dispose()
        {
            _client.RemoveCallbackTarget(this);

            //remove any left over callbacks
            onRoomPropertiesChanged = null;
            onPlayerPropertiesChanged = null;

            onPlayerEnteredRoom = null;
            onPlayerLeftRoom = null;

            onJoinedRoom = null;
            onJoinRoomFailed = null;
            onLeftRoom = null;
            
            onConnect = null;
            onDisconnect = null;
        }
        
        #region CLIENT
        public IEnumerator WaitForConnectedAndReady()
        {
            //TODO: now only called while connecting, should also be called in between room switching and possibly other calls
            //wait for networker and chat networker to both connect (chat optional)
            while (!ConnectedAndReady) {
                yield return null;
            }
            
            onConnectedAndReady?.Invoke();
            onConnectedAndReady = null;
        }
        
        #endregion

        #region CONNECTION
        public void OnConnected()
        {
            Debug.Log($"realtime {_client.NickName} connected!");
            onConnect?.Invoke();
        }

        public void OnConnectedToMaster()
        {
            
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            onDisconnect?.Invoke(cause);
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
            
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
            Debug.LogError($"Custom Authentication failed {debugMessage}");
        }
        #endregion

        #region WEB_RPC
        /*public void OnWebRpcResponse(WebRpcResponse response)
        {
            if (response.ResultCode == 0) {
                onWebRpcResponse?.Invoke(response);
            }
        }*/
        
        public void OnMasterClientSwitched(Player newMasterClient)
        {
            //TODO: Network management
            //throw new System.NotImplementedException();
        }
        #endregion

        #region PLAYER
        public void OnPlayerPropertiesUpdate(Player player, Hashtable changedProps)
        {
            onPlayerPropertiesChanged?.Invoke(player, changedProps);
        }
        
        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {
            //TODO: playermanagement
            //throw new System.NotImplementedException();
        }
        
        public void OnPlayerEnteredRoom(Player player)
        {
            onPlayerEnteredRoom?.Invoke(player);
        }

        public void OnPlayerLeftRoom(Player player)
        {
            onPlayerLeftRoom?.Invoke(player);
        }
        
        #endregion

        #region ROOM
        public void OnJoinedRoom()
        {
            //if chat not connected yet, wait with join on chat connection
            if (!ConnectedAndReady) {
                onConnectedAndReady += onJoinedRoom;
            }
            else {
                onJoinedRoom?.Invoke();    
            }
            Debug.Log("Joined photon room");
        }
        
        /// <summary>
        /// Called when there are changes in the custom properties of the room (network changes).
        /// </summary>
        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            onRoomPropertiesChanged?.Invoke(propertiesThatChanged);
        }
        
        public void OnCreatedRoom()
        {
            //TODO: Network management
            Debug.Log($"created room");
            onRoomCreated?.Invoke();
        }
        
        public void OnLeftRoom()
        {
            onLeftRoom?.Invoke();
        }
        
        public void OnJoinRoomFailed(short returnCode, string message)
        {
            if (!_network.RoomManager.ingnoreJoinError) {
                Debug.LogError($"Join room call failure: ({returnCode}) {message}");    
            }
            onJoinRoomFailed?.Invoke();
        }
        
        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Create room call failure: ({returnCode}) {message}");
            onRoomCreateFailed?.Invoke();
        }
        
        public void OnJoinRandomFailed(short returnCode, string message)
        {
            //TODO: Join random is never called.
            //throw new System.NotImplementedException();
            Debug.LogError($"Join (random) room call failure: ({returnCode}) {message}");
        }
        #endregion
    }
}