using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.Networking.Players {
    /// <summary>
    /// Networking core for player classes
    /// </summary>
    public interface IPlayer {
        /// <summary>
        /// Offers quick access to internal components that require the local player object.
        /// </summary>
        public static IPlayer Local { get; protected set; }

        public Photon.Realtime.Player PhotonPlayer { get; set; }
        public int ActorNumber { get; }
        public bool Initialized { get; set; }
        
        /// <summary>
        /// Call used to initialize this instance of the player.
        /// </summary>
        /// <param name="photonPlayer"></param>
        public void Initialize(Photon.Realtime.Player photonPlayer) {
            PhotonPlayer = photonPlayer;
            
            Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
            if (Network.Local.Client.InRoom) {
                OnJoinedRoom();
            }

            Initialized = true;
        }

        public void RemoveCallbacks() {
            Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
            
            Initialized = false;
        }

        public void Destroy();

        public void OnJoinedRoom();

        public void OnPropertiesChanged(Hashtable changes);
    }
}