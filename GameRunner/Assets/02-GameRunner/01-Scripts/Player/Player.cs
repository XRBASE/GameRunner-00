using Cohort.Networking.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.GameRunner.Players {
    public class Player : MonoBehaviour, IPlayer {
        public Photon.Realtime.Player PhotonPlayer { get; set; }

        [SerializeField] protected string _userName;

        public virtual void OnJoinedRoom() {
            OnPropertiesChanged(PhotonPlayer.CustomProperties);
        }

        public virtual void OnPropertiesChanged(Hashtable changes) {
            string key = Keys.Get(Keys.Player.Name);
            if (changes.ContainsKey(key)) {
                _userName = (string)changes[key];
            }
        }

        public virtual bool SetCustomProperties(Hashtable changes) {
            return PhotonPlayer.SetCustomProperties(changes);
        }
    }
}