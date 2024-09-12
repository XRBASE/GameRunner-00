using System;
using Cohort.Networking.PhotonKeys;
using Cohort.CustomAttributes;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cohort.GameRunner.Avatars {
    public class SceneAvatar : MonoBehaviour {
        /// <summary>
        /// Data with which the avatar can be loaded in.
        /// </summary>
        public PrefabImportData Data {
            get;
            private set;
        }
        
        [Tooltip("Avatar prefab object, should include an animator component.")]
        [SerializeField] private GameObject _avatar;
        [Tooltip("Should the avatar remain visible in the scene, or should it be hidden.")]
        [SerializeField] private bool _avatarVisible;
        [Tooltip("Gender of the avatar 0 = fem, 1 = masc.")]
        [SerializeField] private float _gender = 0.5f;
        [Tooltip("Unique identifier for loading and copying this avatar.")]
        [ReadOnly, SerializeField] private string _guid;
        
        private bool _preloaded;
        private Avatar _instance;
        private AvatarData _data;
        
        private void Awake() {
            //create network data instance
            _data = new AvatarData("", ImportData.AvatarType.Prefab, _guid, _gender);
            //create import data instance
            Data = (PrefabImportData)_data.GetData();
        
            //specific values for the prefab import option avatars that are required pre-import.
            Data.prefab = _avatar;
            Data.preloaded = _avatar.scene.IsValid();
        
            //imports the avatar, so that when it is needed it wil be available within the avatar library.
            AvatarImporter.Instance.ImportAvatar(Data, OnAvatarImported, transform);
        }
        
        private void OnDestroy() {
            AvatarImporter.Instance.RemoveFromLibrary(_instance);
        }
        
        /// <summary>
        /// Sets the component data for this class.
        /// </summary>
        /// <param name="prefab">gameobject or prefab of the avatar that is being loaded, requires animator.</param>
        /// <param name="visible">Should the avatar be visible in the scene.</param>
        /// <param name="guid">Unique avatar identifier.</param>
        /// <param name="gender">Gender of avatar 0 = fem, 1 = masc.</param>
        public void SetData(GameObject prefab, bool visible, string guid, float gender) {
            _avatar = prefab;
            _avatarVisible = visible;
            _guid = guid;
            _gender = gender;
        }
        
        private void OnAvatarImported(Avatar avatar) {
            _instance = avatar;
        
            AvatarImporter.Instance.AddToLibrary(_instance);
            gameObject.SetActive(_avatarVisible);
        }
        
        /// <summary>
        /// Sets the json data in the players custom properties and loads the new avatar.
        /// </summary>
        /// <param name="player">Photon player of player for which the avatar needs to be enabled (local only).</param>
        /// <param name="enabled">True/False set this avatar, or return to server-saved avatar.</param>
        public void EnableForLocalPlayer(Player player, bool enabled) {
            if (!player.IsLocal)
                throw new Exception("Cannot change avatar of remote players.");
        
            Hashtable changes = new Hashtable();
            if (enabled) {
                changes[Keys.Get(Keys.Player.Avatar)] = JsonUtility.ToJson(_data);
            }
            else {
                changes[Keys.Get(Keys.Player.Avatar)] = null;
            }
        
            player.SetCustomProperties(changes);
        }
        
        
#if UNITY_EDITOR
        [CustomEditor(typeof(SceneAvatar))]
        private class SceneAvatarEditor : Editor {
            private SceneAvatar _instance;

            private void OnEnable() {
                _instance = (SceneAvatar)target;
            }

            public override void OnInspectorGUI() {
                DrawDefaultInspector();
                if (GUILayout.Button("Regenerate GUID")) {
                    _instance._guid = Guid.NewGuid().ToString();
                    EditorUtility.SetDirty(_instance);
                }
            }
        }
#endif
    }
}