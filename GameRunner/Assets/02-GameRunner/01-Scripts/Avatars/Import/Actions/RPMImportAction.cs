using System;
using UnityEngine;
using ReadyPlayerMe.Core;

namespace Cohort.GameRunner.Avatars {
    /// <summary>
    /// Used to import RPM avatars
    /// </summary>
    public class RPMImportAction : ImportAction {
        private RPMImportData _data;

        public RPMImportAction(RPMImportData data) : base() {
            _data = data;
        }

        /// <summary>
        /// Start importing avatar using data of this class.
        /// </summary>
        /// <param name="onAvatarImported">Callback when avatar has been imported.</param>
        /// <param name="parent">Parent transform object.</param>
        public override void Execute(Action<Avatar> onAvatarImported, Transform parent) {
            base.Execute(onAvatarImported, parent);

            var loader = new AvatarObjectLoader();
            loader.OnCompleted += OnImportFinished;
            loader.OnFailed += OnImportFailed;

            loader.LoadAvatar(_data.url);
        }

        private void OnImportFinished(object sender, CompletionEventArgs e) {
            Avatar ravatar = e.Avatar.AddComponent<Avatar>();
            ravatar.transform.SetParent(_parent, false);

            //set gender based on avatar outfit.
            if (_data.Gender < -0.5f) {
                switch (e.Metadata.OutfitGender) {
                    case OutfitGender.Feminine:
                        _data.Gender = 0.0f;
                        break;
                    case OutfitGender.Neutral:
                    default:
                        _data.Gender = 0.5f;
                        break;
                    case OutfitGender.Masculine:
                        _data.Gender = 1.0f;
                        break;
                }
            }

            ravatar.Build(_data.Gender, _data.GUID);
            OnImportFinished(ravatar);
        }

        private void OnImportFailed(object sender, FailureEventArgs e) {
            throw new Exception($"Importing (RPM) avatar failed: ({e.Type}) {e.Message},\n{e.Url}.");
        }
    }
}