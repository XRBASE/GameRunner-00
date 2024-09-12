using System;
using Cohort.GameRunner.AvatarAnimations;
using Cohort.Import.GLB;
using Cohort.Ravel.Assets;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
    public class GLBImportAction : ImportAction {
        private GLBImportData _data;

        public GLBImportAction(GLBImportData data) : base() {
            _data = data;
        }
        
        /// <summary>
        /// Start importing avatar using data of this class.
        /// </summary>
        /// <param name="onAvatarImported">Callback when avatar has been imported.</param>
        /// <param name="parent">Parent transform object.</param>
        public override void Execute(Action<Avatar> onAvatarImported, Transform parent) {
            base.Execute(onAvatarImported, parent);

            Asset asset = new Asset(_data.GUID, $"Avatar_{_data.GUID}");
            DataServices.Assets.DownloadAssetData(asset, OnAssetDownloaded, Debug.LogError);
        }
        
        private void OnAssetDownloaded(Asset asset) {
            GameObject arm = new GameObject("armature");
            GLBImporter.Instance.ImportGLBFile(asset.Data, arm.transform, ImportAvatar);
        }
        
        private void ImportAvatar(GameObject avatarObj) {
            Avatar avatar = avatarObj.AddComponent<Avatar>();
            
            Animator anim = avatar.gameObject.AddComponent<Animator>();
            anim.avatar = AvatarBuilder.BuildHumanAvatar(avatar.gameObject, AnimationManager.Instance.GLBAvatar.humanDescription);
            avatar.transform.SetParent(_parent, false);
            
            OnImportFinished(avatar);
        }

        protected override void OnImportFinished(Avatar avatar) {
            avatar.transform.SetParent(_parent, false);
            
            avatar.Build(_data.Gender, _data.GUID);
            base.OnImportFinished(avatar);
        }
    }
}