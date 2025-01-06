using System;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
	public class PrefabImportAction : ImportAction {
		private PrefabImportData _data;
        
        public PrefabImportAction(PrefabImportData data) : base() {
            _data = data;
        }
        
        public override void Execute(Action<Avatar> onAvatarImported, Transform parent) {
            base.Execute(onAvatarImported, parent);
            
            GameObject obj;
            if (!_data.preloaded && !string.IsNullOrEmpty(_data.resourcePath)) {
                obj = LoadFromResources();
            }
            else {
                obj = LoadFromPrefab();
            }
            
            OnImportFinished(obj);
        }

        protected void OnImportFinished(GameObject obj) {
            Avatar ravatar = obj.AddComponent<Avatar>();
            ravatar.Build(_data.Gender, _data.GUID);
            
            base.OnImportFinished(ravatar);
        }

        private void OnImportFailed(string message) {
            throw new Exception($"Importing (Prefab) Ravatar failed: {message}.");
        }
        
        /// <summary>
        /// Loads the avatar from a prefixed position in a Resource folder.
        /// This function is only used if preloaded is false. 
        /// </summary>
        private GameObject LoadFromResources() {
            GameObject obj = GameObject.Instantiate(Resources.Load<GameObject>(_data.resourcePath), _parent);
            if (obj == null) {
                OnImportFailed($"No avatar resource found at {_data.resourcePath}");
            }

            return obj;
        }
        
        /// <summary>
        /// Loads an avatar using a prefab or gameobject. The object is only instantiated if the preloaded check has not
        /// been set. 
        /// </summary>
        /// <returns></returns>
        private GameObject LoadFromPrefab() {
            GameObject obj;
            if (!_data.preloaded) {
                obj = GameObject.Instantiate(_data.prefab, _parent);
            }
            else {
                obj = _data.prefab;
                obj.transform.SetParent(_parent, false);
            }
            
            if (obj == null) {
                OnImportFailed($"No avatar prefab in data!");
            }

            return obj;
        }
	}
}