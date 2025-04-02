using System;
using System.Collections.Generic;
using Cohort.GameRunner.Audio.Minigames;
using Cohort.Patterns;
using UnityEngine;
using UnityEngine.Audio;
using Range = MathBuddy.FloatExtentions.Range;

namespace Cohort.GameRunner.Audio
{
    [DefaultExecutionOrder(100)]
    public class AudioManager : Singleton<AudioManager>
    {
        private const string CHANNEL_VOL_POSTFIX = "Volume";
        private const string PLAYERPREF_KEY = "AudioSettings_";

        public MinigameAudioSet DefaultMinigameAudio
        {
            get { return _defaultMinigameAudio; }
        }

        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioSourceController _template;
        [SerializeField] private MinigameAudioSet _defaultMinigameAudio;

        private Dictionary<Channel, AudioMixerGroup> _groups;
        private ObjectPool<AudioData, AudioSourceController> _sourcePool;
        private Range _dbRange = new Range(-80f, 0f);


        protected override void Awake()
        {
            base.Awake();

            _groups = new Dictionary<Channel, AudioMixerGroup>();
            _sourcePool = new ObjectPool<AudioData, AudioSourceController>(_template);

            //match all mixer groups under master to their respective enum for easy access.
            foreach (var group in _mixer.FindMatchingGroups("Master/"))
            {
                if (!Enum.TryParse(group.name, out Channel channel))
                {
                    Debug.LogError($"Could not find enum for channel {group.name}!");
                    continue;
                }
                _groups[channel] = group;
            }

        }

        private void Start()
        {
            foreach (Channel channel in Enum.GetValues(typeof(Channel)))
            {
                LoadVolume(channel);
            }
        }

        /// <summary>
        /// Creates an audio source based on template and uses it to play the clip on.
        /// Audiosources are pooled and reused when needed.
        /// </summary>
        public AudioSourceController PlayClip(ClipEntry clip, Channel channel, float volume = 1f)
        {
            AudioData data = GetAudioData(clip, channel, volume);

            return _sourcePool.AddItem(data);
        }

        public AudioData GetAudioData(ClipEntry _clip, Channel channel, float volume = 1f)
        {
            return new AudioData(_groups[channel], _clip, volume);
        }

        /// <summary>
        /// Plays audioclip on specified audiosource, so a source in the scene or with specific settings can be used.
        /// </summary>
        public void PlayClip(ClipEntry clip, Channel channel, AudioSourceController source, float volume = 1f,
            bool oneShot = false)
        {
            AudioData data = GetAudioData(clip, channel, volume);
            source.SetData(data);
        }

        /// <summary>
        /// Sets volume for given channel.
        /// </summary>
        /// <param name="volume">volume in decibel.</param>
        /// <param name="channel">Channel of which the volume is being set.</param>
        private void SetAudioVolume(float volume, Channel channel)
        {
            _mixer.SetFloat(channel + CHANNEL_VOL_POSTFIX, volume);
        }

        private float ScaleToVolume(float volume)
        {
            return Mathf.Log10(volume) * 20;
        }

        private void SaveVolume(float volume, Channel channel)
        {
            PlayerPrefs.SetFloat(PLAYERPREF_KEY + channel, volume);
            PlayerPrefs.Save();
        }

        public void HandleVolumeChanged(float volume, Channel channel)
        {
            volume = ScaleToVolume(volume);
            if (volume <= -80f)
            {
                volume = -80f;
            }
            SetAudioVolume(volume, channel);
            SaveVolume(volume, channel);
        }

        private void LoadVolume(Channel channel)
        {
            float volume = PlayerPrefs.GetFloat(PLAYERPREF_KEY + channel);
            SetAudioVolume(volume, channel);
        }


        public float GetAudioVolume(Channel channel)
        {
            _mixer.GetFloat(channel + CHANNEL_VOL_POSTFIX, out float volume);
            return LogarithmicToLinear(volume);
        }

        private float GetDBValue(float percentage)
        {
            return _dbRange.GetValue(percentage);
        }

        static float LogarithmicToLinear(float y)
        {
            return (float) Math.Pow(10, y / 20);
        }

        //Matches channel names in mixer for easy access.
        public enum Channel
        {
            Master = 0,
            Music,
            Sfx
        }
    }

    public struct AudioData
    {
        public ClipEntry clipdata;
        public AudioMixerGroup channel;

        public float volume;

        public AudioData(AudioMixerGroup channel, ClipEntry clipdata, float volume = 1f)
        {
            this.channel = channel;
            this.clipdata = clipdata;
            this.volume = volume;
        }
    }
}