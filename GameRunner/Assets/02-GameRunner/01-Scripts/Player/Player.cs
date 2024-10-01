using Avatar = Cohort.GameRunner.Avatars.Avatar;
using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Avatars;
using Cohort.Networking.Players;
using ExitGames.Client.Photon;
using UnityEngine;
using System;

namespace Cohort.GameRunner.Players {
    public class Player : MonoBehaviour, IPlayer {
        public const int LAYER = 3;
        
        public static LocalPlayer Local { get { return IPlayer.Local as LocalPlayer; } }

        public Photon.Realtime.Player PhotonPlayer { get; set; }
        public int ActorNumber { get { return PhotonPlayer.ActorNumber; } }

        public bool Initialized { get; set; }

        public Hashtable CustomProperties { get { return PhotonPlayer.CustomProperties; } }

        public Avatar Avatar { get; private set; }

        public virtual bool Visible { get { return _visible; } }

        public Action<Hashtable> onPropertiesChanged;
        public Action<Avatar> onAvatarImported;

        [SerializeField] protected string _userName;
        [SerializeField] protected string _avatarUrl;
        [SerializeField] protected Transform _avatarParent;

        private bool _visible = true;

        public void Destroy() {
            ((IPlayer)this).RemoveCallbacks();
            
            GameObject.Destroy(gameObject);
        }

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
            Avatar.SetVisible(_visible);
            onAvatarImported?.Invoke(Avatar);
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
            
            key = Keys.Get(Keys.Player.AvatarVisible);
            if (changes.ContainsKey(key)) {
                _visible = (bool)changes[key];
                if (Avatar != null) {
                    Avatar.SetVisible(_visible);
                }
            }
            
            onPropertiesChanged?.Invoke(changes);
        }

        public virtual bool SetCustomProperties(Hashtable changes) {
            return PhotonPlayer.SetCustomProperties(changes);
        }
    }
}