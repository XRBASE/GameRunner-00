using UnityEngine;

namespace Cohort.GameRunner.Audio.Minigames {
    public class MinigameAudioHandle : MonoBehaviour {
        [SerializeField, Tooltip("Overrides the default audiosources, used by the audio manager.")]
        private AudioSourceController _sourceOverride;

        [SerializeField, Tooltip("Overrides the default audioclips, used by the audio manager.")]
        private MinigameAudioSet _audioOverrides;
        
        public AudioSourceController PlayClip(AudioType type) {
            if (_audioOverrides == null || !_audioOverrides.TryGetClip(type, out MingameAudioClipEntry clip)) {
                if (!AudioManager.Instance.DefaultMinigameAudio.TryGetClip(type, out clip)) {
                    Debug.LogError($"Missing audio clip, No clip found for type: {type}!");
                    return null;
                }
            }

            if (_sourceOverride != null) {
                AudioManager.Instance.PlayClip(clip, AudioManager.Channel.Minigame, _sourceOverride);
                return _sourceOverride;
            }
            else {
                return AudioManager.Instance.PlayClip(clip, AudioManager.Channel.Minigame);
            }
        }
    }
}