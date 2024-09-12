using System.Security.Cryptography;
using Cohort.GameRunner.Avatars;
using Cohort.Networking.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;
using Avatar = Cohort.GameRunner.Avatars.Avatar;

namespace Cohort.GameRunner.Players {
    public class Player : MonoBehaviour, IPlayer {
        public Photon.Realtime.Player PhotonPlayer { get; set; }

        public Avatar Avatar { get; private set; }

        [SerializeField] protected string _userName;
        [SerializeField] protected string _avatarUrl;
        [SerializeField] protected Transform _avatarParent;

        protected void ImportAvatar() {
            if (string.IsNullOrEmpty(_avatarUrl)) {
                AvatarImporter.Instance.ImportTemplate(OnAvatarImported, _avatarParent);
            }
            else {
                AvatarImporter.Instance.ImportAvatar(_avatarUrl, OnAvatarImported, _avatarParent);
            }
        }

        protected void OnAvatarImported(Avatar avatar) {
            avatar.SetupHelpers(Avatar);
            
            if (Avatar != null) {
                Destroy(Avatar.gameObject);
            }
            
            Avatar = avatar;
            Avatar.SetVisible(true);
        }

        public virtual void OnJoinedRoom() {
            OnPropertiesChanged(PhotonPlayer.CustomProperties);
        }

        public virtual void OnPropertiesChanged(Hashtable changes) {
            string key = Keys.Get(Keys.Player.Name);
            if (changes.ContainsKey(key)) {
                _userName = (string)changes[key];
            }
            
            key = Keys.Get(Keys.Player.Avatar);
            if (changes.ContainsKey(key)) {
                _avatarUrl = (string)changes[key];
                ImportAvatar();
            }
        }

        public virtual bool SetCustomProperties(Hashtable changes) {
            return PhotonPlayer.SetCustomProperties(changes);
        }
    }
}