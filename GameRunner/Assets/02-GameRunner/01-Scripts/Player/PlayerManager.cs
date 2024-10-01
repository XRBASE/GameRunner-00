using System.Collections.Generic;
using Cohort.Networking.Players;
using UnityEngine;

namespace Cohort.GameRunner.Players {
    public class PlayerManager : Networking.Players.PlayerManager {
        public new static PlayerManager Instance {
            get { return (PlayerManager)Networking.Players.PlayerManager.Instance; }
        }

        [SerializeField] private LocalPlayer _localPlayerPrefab;
        [SerializeField] private Player _playerPrefab;
        
        protected override IPlayer CreatePlayer(Photon.Realtime.Player photon) {
            if (photon.IsLocal) {
                return IPlayer.Local;
            }
            
            return Instantiate(_playerPrefab);
        }

        public bool ActorNumberExists(int playerNumber) {
            foreach (var kv_player in _players) {
                if (kv_player.Value.ActorNumber == playerNumber) {
                    return !kv_player.Value.PhotonPlayer.IsInactive;
                }
            }

            return false;
        }
    }
}