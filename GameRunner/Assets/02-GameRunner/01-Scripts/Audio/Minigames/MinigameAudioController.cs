using UnityEngine;

namespace Cohort.GameRunner.Audio.Minigames {
	public class MinigameAudioController : MonoBehaviour {
		[SerializeField] private MinigameAudioSet _set;
		
		/// <summary>
		/// Plays default clip matching the given state. Based on given audioset in inspector. 
		/// </summary>
		/// <param name="type">state of which the audio clip will be played.</param>
		/// <param name="controller">out variable that contains controller, with which the clip can be stopped again (in case of looping clips).</param>
		/// <param name="volume">volume parameter for the clip.</param>
		/// <returns>True/False any clip was found in the default audioset.</returns>
		public bool TryPlayClip(AudioType type, out AudioSourceController controller, float volume = 1f) {
			if (_set.TryGetClip(type, out MingameAudioClipEntry clip)) {
				controller = AudioManager.Instance.PlayClip(clip, AudioManager.Channel.Minigame, volume);
				return true;
			}

			controller = null;
			return false;
		}
		
		/// <summary>
		/// Plays specific clip through the audio manager.
		/// </summary>
		/// <param name="clip">audio clip to play</param>
		/// <param name="volume">volume of the specific clip</param>
		/// <param name="loop">should the clip loop? Use controller to stop playing clip.</param>
		/// <param name="oneShot">should clip be played as oneshot? controller will clean itself up.</param>
		/// <returns>Controller with which clips can be stopped.</returns>
		public AudioSourceController PlayClip(AudioClip clip, float volume = 1f, bool loop = false, bool oneShot = false) {
			MingameAudioClipEntry entry = new MingameAudioClipEntry();
			entry.audio = clip;
			entry.oneShot = oneShot;
			entry.loop = loop;
			
			return AudioManager.Instance.PlayClip(entry, AudioManager.Channel.Minigame, volume);
		}
	}
	
	/// <summary>
	/// Types of available audio clips.
	/// </summary>
	public enum AudioType {
		Ambience, //Background
		Success,
		Failure,
	}
}