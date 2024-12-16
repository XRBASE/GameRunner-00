using System;
using System.Collections;
using Cohort.Ravel.PhotonNetworking.Callbacks;
using Photon.Realtime;
using UnityEngine;

namespace Cohort.Ravel.PhotonNetworking.Rooms
{
    /// <summary>
    /// Manager for switching between photon (realtime) rooms. In the flow Space service, will distribute calls to the scene manager for loading an environment
    /// and this class for doing the network sheboopies.
    /// </summary>
    public class RoomManager
    {
        /// <summary>
        /// Id of the current photon room.
        /// </summary>
        public string CurrentRoomId {
            get { return _currentId; }
        }
        private string _currentId;
        private bool _canRejoin;
        private bool _createOrJoinFailed = false;
        
        private PhotonClient _client;
        
        //is private, because this callback is cleared after loading the room. 
        //it cannot be temporary, because it is a photon callback, but is used as a temporary action.
        private Action<string> _onJoinRoomFailed;
        private RealtimeCallbackHandle _callbacks;
        
        public RoomManager(PhotonClient client, RealtimeCallbackHandle rtCallbacks)
        {
            _client = client;
            
            _callbacks = rtCallbacks;
            _callbacks.onRoomCreateFailed += OnRoomCreateFailed;
            _callbacks.onJoinedRoom += OnJoinedRoom;
            _callbacks.onJoinRoomFailed += OnJoinRoomFailed;
        }

        /// <summary>
        /// force leaves the room, should be called before disposing the client.
        /// </summary>
        public void Dispose()
        {
            _onJoinRoomFailed = null;
            if (_client.InRoom) {
                _callbacks.onLeavingRoom?.Invoke();
            }
            
            if (_client.InRoom) {
                lock (_client) {
                    _client.OpLeaveRoom(false);
                    _client.Service();
                    
                    _client = null;
                }

                _currentId = "";
            }
        }

        private void OnJoinedRoom()
        {
            //clear any room failure subscriptions, so old room joins are not shown as failed
            _onJoinRoomFailed = null;
            _canRejoin = true;
        }

        private void OnJoinRoomFailed()
        {
            _createOrJoinFailed = true;
            //call room failed subscriptions, then clear
            _onJoinRoomFailed?.Invoke("Join room failed!");
            _onJoinRoomFailed = null;
            _canRejoin = false;
        }

        /// <summary>
        /// Tries to reconnect/create the room the user was in before disconnecting.
        /// </summary>
        public void OnReconnect()
        {
            if (string.IsNullOrEmpty(_currentId)) {
                Debug.LogError($"cannot connect to previous room, just connecting to the masterserver.");
                return;
            }

            if (_canRejoin) {
                RejoinRoom(_currentId);
            }
            else {
                JoinOrCreateRoom(_currentId);
            }
        }

        /// <summary>
        /// Leaves current room and goes to the next room.
        /// </summary>
        /// <param name="roomId">id of room to join.</param>
        /// <param name="rejoin">(not yet implemented, but needed for )true/false user has already been in this room</param>
        public void GotoRoom(string roomId, bool rejoin = false, bool leaveInactive = true, Action<string> onFailure = null)
        {
            if (!string.IsNullOrEmpty(_currentId)) {
                //in room still
                LeaveRoom();
            }
            
            //failure is used to let backend/host know user didn't join room.
            _onJoinRoomFailed = onFailure;
            
            if (roomId == _currentId)
                return;

            Networker.Instance.StartCoroutine(DoGotoRoom(roomId, rejoin));
        }

        private void OnRoomCreateFailed() {
            _createOrJoinFailed = true;
        }

        //should only be set by this EINumerator as it disables errors in photon callbacks.
        public bool ingnoreJoinError = false;
        private IEnumerator CreateOrRejoin(string roomId)
        {
            while (!(_client.IsConnectedAndReady)) {
                yield return null;
            }
            
            //used to keep track of create or join failure
            _createOrJoinFailed = false;
            ingnoreJoinError = true;
            JoinOrCreateRoom(roomId);
            
            while (!(_client.InRoom || _createOrJoinFailed)) {
                yield return null;
            }
            ingnoreJoinError = false;
            
            //if client in room create successfull, otherwise rejoin
            if (!_client.InRoom) {
                //this version does throw an error, (OnJoinFailed) but connects after that. It just needs the join failure to 
                //check that te client actually needs to rejoin instead of join.
                RejoinRoom(roomId);
            }
        } 
        
        private IEnumerator DoGotoRoom(string roomId, bool rejoin)
        {
            while (!(_client.IsConnectedAndReady)) {
                yield return null;
            }

            if (rejoin) {
                RejoinRoom(roomId);
            }
            else {
                JoinOrCreateRoom(roomId);
            }
        }
        
        /// <summary>
        /// Leave the currently joined room and return to being connected to the master server.
        /// </summary>
        public void LeaveRoom()
        {
            if (!_client.InRoom) {
                //error case, always clear cur id
                _currentId = "";
                return;
            }
            
            _callbacks.onLeavingRoom?.Invoke();
            _currentId = "";
            _canRejoin = false;
            
            _client.OpLeaveRoom(false);
        }
        
        /// <summary>
        /// Join an existing room, that this user has not yet joined (otherwise call rejoin).
        /// </summary>
        /// <param name="roomId">id of the room that will be joined.</param>
        /// <returns>True/False initial part of creation succeeded (can still fail after this return though).</returns>
        private bool RejoinRoom(string roomId)
        {
            _currentId = roomId;
            
            return _client.OpRejoinRoom(roomId);
        }
        
        /// <summary>
        /// Join an existing room or create one if the existing one does not exist.
        /// </summary>
        /// <param name="roomId">roomid of photon room that needs to be created.</param>
        /// <returns>True/False initial part of creation succeeded (can still fail after this return though).</returns>
        private bool JoinOrCreateRoom(string roomId)
        {
            RoomOptions options = new RoomOptions() {
                //MaxPlayers
                //InitialCustomRoomProperties
                //CustomRoomPropertiesForLobby
                PlayerTtl = -1,
                EmptyRoomTtl = 0,
                PublishUserId = true,

            };
            
            _currentId = roomId;
            //return _client.OpCreateRoom(new EnterRoomParams() {RoomName = roomId, RoomOptions = options});
            return _client.OpJoinOrCreateRoom(new EnterRoomParams() {RoomName = roomId, RoomOptions = options});
        }
    }
}