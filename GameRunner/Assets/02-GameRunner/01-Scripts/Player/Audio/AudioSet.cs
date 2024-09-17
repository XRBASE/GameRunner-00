using System;
using System.Collections.Generic;
using Cohort.GameRunner.AvatarAnimations;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "SpaceShift/Audio/CharacterAudioSet", fileName = "CharAudio_")]
public class AudioSet : ScriptableObject {
    [SerializeField] private List<ClipEntry> _entries;
    
    public bool TryGetClip(CharAnimator.AnimationState state, out ClipEntry clip) {
        for (int i = 0; i < _entries.Count; i++) {
            if (_entries[i].state == state) {
                clip = _entries[i];
                return true;
            }
        }

        clip = null;
        return false;
    }

    [Serializable]
    public class ClipEntry {
        public CharAnimator.AnimationState state;
        public AudioClip audio;
        public bool loop;
        public bool oneShot;
        
        [Space]
        public bool scaled;
        public float pitchflux = 0f;
        public string scaleParamName;
        [FormerlySerializedAs("scaleOffset")] public float basePitch = 1.0f;
    }
    
#if UNITY_EDITOR
    public bool CheckForDoubleEntries(out CharAnimator.AnimationState state) {
        for (int i = 0; i < _entries.Count; i++) {
            for (int j = i + 1; j < _entries.Count; j++) {
                if (_entries[i].state == _entries[j].state) {
                    state = _entries[i].state;
                    return true;
                }
            }
        }

        state = CharAnimator.AnimationState.Unknown;
        return false;
    }
    
    [CustomEditor(typeof(AudioSet))]
    private class AudioSetEditor : Editor {
        private AudioSet _instance;
        
        private bool _hasDouble = false;
        private CharAnimator.AnimationState _state;

        private void OnEnable() {
            _instance = (AudioSet)target;
            _hasDouble = _instance.CheckForDoubleEntries(out _state);
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            
            if (EditorGUI.EndChangeCheck()) {
                _hasDouble = _instance.CheckForDoubleEntries(out _state);
            }

            if (_hasDouble) {
                EditorGUILayout.HelpBox($"Double state entry found for state: {_state} found.\n These sounds will not be played.", MessageType.Error);
            }
        }
    }
#endif
}
