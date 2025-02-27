using System;
using System.Collections.Generic;
using Cohort.Patterns;
using UnityEngine;
using UnityEngine.Audio;
using Range = MathBuddy.FloatExtentions.Range;

namespace Cohort.GameRunner.Audio {
    public class AudioManager : Singleton<AudioManager> {
        private const string CHANNEL_VOL_POSTFIX = "Volume"; 
        
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSourceController _template;

        private Dictionary<Channel, AudioMixerGroup> _groups;
        private ObjectPool<AudioData, AudioSourceController> _sourcePool;
        private Range _dbRange = new Range(-80f, 20f);

        private void Awake() {
            //match all mixer groups under master to their respective enum for easy access.
            foreach (var group in _mixer.FindMatchingGroups("Master/")) {
                if (!Enum.TryParse(group.name, out Channel channel)) {
                    Debug.LogError($"Could not find enum for channel {group.name}!");
                    continue;
                }

                _groups[channel] = group;
            }
        }
        
        /// <summary>
        /// Creates an audio source based on template and uses it to play the clip on.
        /// Audiosources are pooled and reused when needed.
        /// </summary>
        public AudioSourceController PlayClip(ClipEntry _clip, Channel channel, float volume = 1f) {
            AudioData data = new AudioData(_groups[channel], _clip, volume);
            
            return _sourcePool.AddItem(data);
        }
        
        /// <summary>
        /// Plays audioclip on specified audiosource, so a source in the scene or with specific settings can be used.
        /// </summary>
        public void PlayClip(ClipEntry clip, Channel channel, AudioSource source, float volume = 1f, bool oneShot = false) {
            source.clip = clip.audio;
            source.loop = clip.loop;
			
            source.volume = volume;
            source.outputAudioMixerGroup = _groups[channel];
			
            if (!source.loop && oneShot) {
                source.PlayOneShot(clip.audio);
            }
            else {
                source.Play();
            }
        }

        /// <summary>
        /// Sets volume for given channel.
        /// </summary>
        /// <param name="volume">Percentage of wanted volume in decimals (between 0.0 and 1.0).</param>
        /// <param name="channel">Channel of which the volume is being set.</param>
        public void SetAudioVolume(float volume, Channel channel) {
            
            _mixer.SetFloat(channel + CHANNEL_VOL_POSTFIX, GetDBValue(volume));
        }

        private float GetDBValue(float percentage) {
            return _dbRange.GetValue(percentage);
        }

        //Matches channel names in mixer for easy access.
        public enum Channel {
            Environment = 0,
            Minigame,
            Player
        }
    }

    public struct AudioData {
        public ClipEntry clipdata;
        public AudioMixerGroup channel;
        
        public float volume;

        public AudioData(AudioMixerGroup channel, ClipEntry clip, float volume = 1f) {
            this.channel = channel;
            this.clipdata = clip;
            this.volume = volume;
        }
    }
}