using System;
using UnityEngine;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Cohort.Patterns;

namespace Cohort.Networking.Players {
    [DefaultExecutionOrder(-2)] //after players
    public abstract class PlayerManager : Singleton<PlayerManager> {
        /// <summary>
        /// Player dictionary indexed by their userId
        /// </summary>
        protected Dictionary<string, IPlayer> _players = new Dictionary<string, IPlayer>();
        
        /// <summary>
        /// on customprops callbacks for all players in the room, indexed by userid.
        /// </summary>
        private Dictionary<string, Action<Hashtable>> _propertyCallbacks = new Dictionary<string, Action<Hashtable>>();
        
        protected void Start() {
            Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
            Network.Local.Callbacks.onPlayerEnteredRoom += AddPlayer;
            Network.Local.Callbacks.onPlayerLeftRoom += RemovePlayer;
            Network.Local.Callbacks.onPlayerPropertiesChanged += PlayerPropertiesChanged;
        }
        
        protected void OnDestroy()
        {
            if (!Networker.Instance.Disposed)
            {
                Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
                Network.Local.Callbacks.onPlayerEnteredRoom -= AddPlayer;
                Network.Local.Callbacks.onPlayerLeftRoom -= RemovePlayer;
                Network.Local.Callbacks.onPlayerPropertiesChanged -= PlayerPropertiesChanged;
            }
        }
        
        private void OnJoinedRoom() {
            foreach (var kv_player in Network.Local.Client.CurrentRoom.Players) {
                if (!kv_player.Value.IsInactive) {
                    AddPlayer(kv_player.Value);
                }
            }
        }
        
        private void AddPlayer(Photon.Realtime.Player photon) {
            IPlayer player;
            
            player = CreatePlayer(photon);
            player.Initialize(photon);
            
            if (_players.ContainsKey(photon.UserId)) {
                Debug.LogWarning("Double player detected. oldest player removed!");
                _players[photon.UserId].Destroy();
            }
            
            _players[photon.UserId] = player;
            _propertyCallbacks[photon.UserId] = player.OnPropertiesChanged;
        }
        
        private void RemovePlayer(Photon.Realtime.Player player) {
            _players[player.UserId].Destroy();
            _players.Remove(player.UserId);
        }
        
        private void PlayerPropertiesChanged(Photon.Realtime.Player player, Hashtable changes) {
            if (_propertyCallbacks.ContainsKey(player.UserId)) {
                _propertyCallbacks[player.UserId]?.Invoke(changes);
            }
        }
        
        protected abstract IPlayer CreatePlayer(Photon.Realtime.Player photon);
    }
}