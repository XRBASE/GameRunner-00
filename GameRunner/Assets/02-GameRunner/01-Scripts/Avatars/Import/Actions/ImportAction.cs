using System;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
    public abstract class ImportAction {
        private Action<Avatar> onImportComplete;
        protected Transform _parent;
        
        /// <summary>
        /// This function starts the process of importing the avatar, based on the data set in the action.
        /// </summary>
        /// <param name="onAvatarImported">Callback that is fired when the import process has been completed.</param>
        public virtual void Execute(Action<Avatar> onAvatarImported, Transform parent) {
            _parent = parent;
            onImportComplete = onAvatarImported;
        }

        /// <summary>
        /// Call that happens when importing the avatar has been completed.
        /// </summary>
        /// <param name="avatar"></param>
        protected virtual void OnImportFinished(Avatar avatar) {
            onImportComplete?.Invoke(avatar);
        }
        
        /// <summary>
        /// Get import action matching the data class. This automatically sorts what data type should be used. 
        /// </summary>
        /// <param name="data">Data class containing import information.</param>
        /// <returns></returns>
        /// <exception cref="Exception">Type not implemented exception, happens when the given type of action has not yet been implemented.</exception>
        public static ImportAction GetAction(ImportData data) {
            switch (data.Type) {
                case ImportData.AvatarType.ReadyPlayerMe:
                    return new RPMImportAction((RPMImportData)data);
                case ImportData.AvatarType.Prefab:
                    return new PrefabImportAction((PrefabImportData)data);
                case ImportData.AvatarType.Glb:
                    return new GLBImportAction((GLBImportData)data);
                default:
                    Debug.LogError($"No import action implemented for type: {data.Type}");
                    return null;
            }
        }
    }
}