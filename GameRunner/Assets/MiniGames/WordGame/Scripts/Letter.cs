using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Letter : MonoBehaviour {
	public enum LetterState{Incorrect, Correct, Contains}

	public Image image;
	public TextMeshProUGUI textMeshPro;
	public PlayableDirector playableDirector;
	public PlayableAsset bouncePlayable;
	public PlayableAsset rotationPlayable;
	public PlayableAsset wrongPlayable;
	public AudioClip addSoundEffect;
	public AudioClip removeSoundEffect;


	private AudioSource _audioSource;
	private Color _correctColor = Color.green;
	private Color _incorrectColor = Color.grey;
	private Color _includedColor = Color.yellow;
	private int _index;
	private char _letter;

	public void SetLetterIndex(int index) {
		_index = index;
	}

	public void SetLetterText(char letter) {
		_letter = letter;
		textMeshPro.text = $"{_letter}";
	}

	public void SetAudioSource(AudioSource audioSource)
	{
		_audioSource = audioSource;
	}
	
	public void AddFeedback()
	{
		StartPlayable(bouncePlayable);
		_audioSource.PlayOneShot(addSoundEffect);
	}

	public void RemoveFeedback()
	{
		_audioSource.PlayOneShot(removeSoundEffect);
	}

	public void CorrectFeedback()
	{
		StartPlayable(rotationPlayable);
	}

	public void IncorrectFeedback()
	{
		StartPlayable(wrongPlayable);
	}

	private void StartPlayable(PlayableAsset playable)
	{
		playableDirector.time = 0;
		playableDirector.Stop();
		playableDirector.Evaluate();
		playableDirector.playableAsset = playable;
		playableDirector.Play();
	}

	public bool CheckLetter(string correctWord) {
		if (correctWord.Contains(_letter)) {
			if (correctWord[_index] == _letter) {
				HandleLetterState(LetterState.Correct);
				return true;
			}
			else {
				HandleLetterState(LetterState.Contains);
			}
		}
		else {
			HandleLetterState(LetterState.Incorrect);
		}
		return false;

	}

	private void HandleLetterState(LetterState letterState) {
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
