using System;
using UnityEngine;

namespace Cohort.GameRunner.Audio.Minigames {
	[Serializable]
	public class MingameAudioClipEntry : ClipEntry<AudioType> { }
	
	[CreateAssetMenu(menuName = "Cohort/Audio/MinigameAudioSet", fileName = "MinigameAudio_")]
	public class MinigameAudioSet : AudioSet<MingameAudioClipEntry, AudioType> { }
	
}