using System;
using UnityEngine;

using Cohort.GameRunner.AvatarAnimations;
using UnityEngine.Serialization;

namespace Cohort.GameRunner.Audio {
    [Serializable]
    public class CharClipEntry : ClipEntry<CharAnimator.AnimationState> {
        [Space]
        public bool scaled;
        public float pitchflux = 0f;
        public string scaleParamName;
        [FormerlySerializedAs("scaleOffset")] public float basePitch = 1.0f;
    }

    [CreateAssetMenu(menuName = "Cohort/Audio/CharacterAudioSet", fileName = "CharAudio_")]
    public class CharacterAudioSet : AudioSet<CharClipEntry, CharAnimator.AnimationState> { }
}