using System.Collections.Generic;
using Cohort.Ravel.Patterns;
using UnityEngine;

namespace Cohort.GameRunner.AvatarAnimations {
    public class AnimationManager : Singleton<AnimationManager> {
        public EmoteSet DefaultEmotes {
            get { return _defaultEmotes; }
        }
        [SerializeField] private EmoteSet _defaultEmotes;

        public RuntimeAnimatorController AnimationController {
            get { return _animationController; }
        }
        [SerializeField] private RuntimeAnimatorController _animationController;
        
        public Avatar GLBAvatar {
            get { return glbAvatar; }
        }
        [SerializeField] private Avatar glbAvatar;
        
        //dictionary containing all emotes in the whole of Ravel.
        private Dictionary<string, Emote> _emotes;
        
        protected override void Awake() {
            base.Awake();

            _emotes = new Dictionary<string, Emote>(EmoteSet.MAX_EMOTES * 2);
            AddEmotes(_defaultEmotes);
        }
        
        /// <summary>
        /// Add singular emote to the list of optional emotes.
        /// </summary>
        public void AddEmote(Emote emote) {
            if (_emotes.ContainsKey(emote.GUID) && emote != _emotes[emote.GUID]) {
                Debug.LogError($"Double emote guid error: emote {emote.clip.name} has the same guid as {_emotes[emote.GUID].clip.name} (guid: {emote.GUID})"); 
            }

            _emotes[emote.GUID] = emote;
        }
        
        /// <summary>
        /// Add set of emotes to the emote list.
        /// </summary>
        /// <param name="set"></param>
        public void AddEmotes(EmoteSet set) {
            for (int i = 0; i < set.Count; i++) {
                AddEmote(set[i]);
            }
        }

        public Emote GetEmote(string guid) {
            //if an emote is missing here, something goes wrong in adding the emotes to this class. This class should always
            //have all emotes available in the dictionary.
            return _emotes[guid];
        }
    }
}