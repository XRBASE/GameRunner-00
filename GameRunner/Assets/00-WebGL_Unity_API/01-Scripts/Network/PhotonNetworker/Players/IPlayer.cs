using ExitGames.Client.Photon;

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
        }

        public void Destroy() {
            Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
        }

        public void OnJoinedRoom();

        public void OnPropertiesChanged(Hashtable changes);
    }
}