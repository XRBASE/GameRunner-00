using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace Cohort.GameRunner.Minigames.Wordle {
	public class Letter : MonoBehaviour {
		public enum LetterState {
			Unset=0,
			Incorrect,
			Correct,
			Contains
		}

		public char letter
		{
			get
			{
				return _letter;
			}
		}

		public LetterState letterState;
		public Image image;
		public TextMeshProUGUI textMeshPro;
		public PlayableDirector playableDirector;
		public PlayableAsset bouncePlayable;
		public PlayableAsset rotationPlayable;
		public PlayableAsset wrongPlayable;
		public AudioClip addSoundEffect;
		public AudioClip removeSoundEffect;


		private AudioSource _audioSource;
		[SerializeField] private Color _correctColor = Color.green;
		[SerializeField] private Color _incorrectColor = Color.grey;
		[SerializeField] private Color _includedColor = Color.yellow;
		private int _index;
		private char _letter;

		public void SetLetterIndex(int index) {
			_index = index;
		}

		public void SetLetterText(char letter) {
			_letter = letter;
			textMeshPro.text = $"{_letter}";
		}

		public void SetAudioSource(AudioSource audioSource) {
			_audioSource = audioSource;
		}

		public void AddFeedback() {
			StartPlayable(bouncePlayable);
			_audioSource.PlayOneShot(addSoundEffect);
		}

		public void RemoveFeedback() {
			_audioSource.PlayOneShot(removeSoundEffect);
		}

		public void CorrectFeedback() {
			StartPlayable(rotationPlayable);
		}

		public void IncorrectFeedback() {
			StartPlayable(wrongPlayable);
		}

		private void StartPlayable(PlayableAsset playable) {
			playableDirector.time = 0;
			playableDirector.Stop();
			playableDirector.Evaluate();
			playableDirector.playableAsset = playable;
			playableDirector.Play();
		}

		public bool CheckLetter(char letter)
		{
			return _letter == letter;
		}

		public void HandleLetterState(LetterState letterState)
		{
			this.letterState = letterState;
			switch (letterState) {
				case LetterState.Incorrect:
					image.color = _incorrectColor;
					break;
				case LetterState.Correct:
					image.color = _correctColor;
					break;
				case LetterState.Contains:
					image.color = _includedColor;
					break;
			}
		}
	}
}