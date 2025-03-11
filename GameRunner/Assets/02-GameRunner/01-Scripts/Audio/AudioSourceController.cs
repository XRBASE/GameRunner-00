using System;
using Cohort.Patterns;
using UnityEngine;

namespace Cohort.GameRunner.Audio {
	public class AudioSourceController : MonoBehaviour, ObjectPool<AudioData, AudioSourceController>.IPoolable {
		public bool IsActive {
			get { return gameObject.activeSelf; }
			set { gameObject.SetActive(value); }
		}

		public AudioSource AudioSource {
			get { return _source; }
		}

		[SerializeField] private AudioSource _source;

		private ObjectPool<AudioData, AudioSourceController> _pool;
		private bool _initialized = false;

		public void Initialize(ObjectPool<AudioData, AudioSourceController> pool) {
			_pool = pool;
		}

		private void Update() {
			if (_initialized && !_source.isPlaying) {
				Stop();
			}
		}

		public void Stop() {
			_source.Stop();
			_pool?.RemoveItem(this);
		}

		public void UpdatePoolable(int index, AudioData data) {
			gameObject.name = $"Audiosource_{index}_{data.clipdata.audio.name}";
				
			SetData(data);
		}

		public void SetData(AudioData data) {
			IsActive = true;
			
			_source.clip = data.clipdata.audio;
			_source.loop = data.clipdata.loop;
			
			_source.volume = data.volume;
			_source.outputAudioMixerGroup = data.channel;
			
			if (!_source.loop && data.clipdata.oneShot) {
				_source.PlayOneShot(data.clipdata.audio);
			}
			else {
				_source.Play();
			}
			_initialized = true;
		}

		public AudioSourceController Copy() {
			return Instantiate(this, transform.parent);
		}
	}
}