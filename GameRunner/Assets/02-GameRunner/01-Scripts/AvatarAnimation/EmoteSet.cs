using System;
using System.Collections.Generic;
using Cohort.CustomAttributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Cohort.GameRunner.AvatarAnimations {
    [CreateAssetMenu(fileName = "Emoteset_", menuName = "Cohort/Emotes", order = 0)]
    public class EmoteSet : ScriptableObject {
        public const int MAX_EMOTES = 10;
        public int Count { get { return _emotes.Count; } }

        public Emote this[int index] {
            get { return _emotes[index]; }
        }

        [SerializeField] private List<Emote> _emotes = new List<Emote>(MAX_EMOTES);
		
        /// <summary>
        /// Tries to retrieve an animation clip based on a given index.
        /// </summary>
        /// <param name="index">Index of clip to be retrieved.</param>
        /// <param name="emote">If true, this contains the found animation clip.</param>
        /// <param name="fullBody">If true, this contains whether the animation clip is marked as a full body animation.</param>
        /// <returns>True/False an animation clip has been found.</returns>
        public bool TryGetEmote(int index, out Emote emote) {
            if (index < _emotes.Count) {
                emote = _emotes[index];
                return true;
            }
			
            emote = null;
            return false;
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(EmoteSet))]
        private class EmoteSetEditor : Editor
        {
            public override void OnInspectorGUI() {
                EmoteSet instance = (EmoteSet)target;
                int prevCount = instance.Count;
				
                EditorGUI.BeginChangeCheck();
                DrawDefaultInspector();
                if (EditorGUI.EndChangeCheck()) {
                    if (instance.Count > MAX_EMOTES) {
                        Debug.LogError($"Cannot add more animations to this set, maximum is set to {MAX_EMOTES}");
                        instance._emotes.RemoveRange(MAX_EMOTES - 1, instance._emotes.Count - MAX_EMOTES);
                    }
					
                    if (prevCount != instance.Count &&
                        instance[^2].GUID == instance[^1].GUID) {
                        instance._emotes[^1] = new Emote();
                    }
                }
            }
        }
#endif
    }
    
    /// <summary>
    /// Simple container class for animation clip data.
    /// </summary>
    [Serializable]
    public class Emote
    {
        public string GUID {
            get { return _guid; }
        }

        [SerializeField] public AnimationClip clip;
        [SerializeField] public bool isFullBody = true;
        [SerializeField, ReadOnly] private string _guid;

        public Emote() {
            _guid = Guid.NewGuid().ToString();
        }
    }
}