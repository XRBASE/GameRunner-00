using Cohort.Networking.Players;
using UnityEngine;

namespace Cohort.GameRunner.Players {
    public class PlayerManager : Networking.Players.PlayerManager {
        [SerializeField] private LocalPlayer _localPlayerPrefab;
        [SerializeField] private Player _playerPrefab;
        
        protected override IPlayer CreatePlayer(Photon.Realtime.Player photon) {
            if (photon.IsLocal) {
                return IPlayer.Local;
            }
            
            return Instantiate(_playerPrefab);
        }
    }
}