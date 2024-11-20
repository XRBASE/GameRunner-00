using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

public class WordGame : Learning
{
    private const float INVALID_FEEDBACK_TIMEOUT = 1f;
    private const float GAME_COMPLETE_FEEDBACK_TIMEOUT = 2f;
    private Word CurrentWord => _words[_entryIndex];
    private bool CanPlay => _isPlaying && _entryIndex < _words.Count;
    
    public Word wordPrefab;
    public WordGameInput wordGameInput;
    public Transform gameParent;
    public TextMeshProUGUI title;
    public AudioSource feedbackAudio;
    public AudioClip successAudioClip, failureAudioClip;
    public ScoreUI scoreUI;
    public TextMeshProUGUI hintText;
    public PlayableDirector invalidWordFeedback;
    
    private readonly List<Word> _words = new List<Word>();
    private List<string> _wordDictionary;
    private WordGameData _wordGameData;
    private Action<float> _onGameFinished;
    private Action<float> _testOnGameFinished;
    private WordData _chosenWord;
    private IEnumerator _feedBackTimeoutRoutine;
    private string _text = string.Empty;
    private int _entryIndex;
    private int _wordLength;
    private float _completionPercent;
    private float _scoreMultiplier;
    private bool _isPlaying;


    public override void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished)
    {
        _wordGameData = JsonUtility.FromJson<WordGameData>(gameData);
        _wordLength = _wordGameData.wordList.First().word.Length;
        _wordDictionary = WordDictionary.GetWordDictionary(_wordLength);
        _onGameFinished = onGameFinished;
        _scoreMultiplier = scoreMultiplier;
        title.text = _wordGameData.title;
        BuildGame(_wordLength, _wordGameData.tries);
    }
    
    protected override void Awake()
    {
        base.Awake();
        wordGameInput.onKeyboardInput += ValueChanged;
        wordGameInput.remove += RemoveLastLetter;
    }

    private void DisplayScore(float score)
    {
        scoreUI.PlayScore((int)(score * _scoreMultiplier));
    }

    private void BuildGame(int wordLength, int tries)
    {
        _words.Clear();
        for (int i = 0; i < tries; i++)
        {
            var word = Instantiate(wordPrefab, gameParent);
            _words.Add(word);
            word.Initialise(wordLength, feedbackAudio);
        }
        _chosenWord = _wordGameData.wordList[Random.Range(0, _wordGameData.wordList.Count)];
        hintText.text = _chosenWord.hint;
        _entryIndex = 0;
        _isPlaying = true;
    }

    private void Submit()
    {
        if (!CanPlay || _text.Length < _wordLength)
            return;
        if (!_wordDictionary.Contains(_text))
        {
            HandleAnswerInvalid();
            return;
        }

        HandleAnswer(CurrentWord.CheckWord(_chosenWord.word));
    }

    private void HandleAnswerInvalid()
    {
        CurrentWord.InCorrectFeedback();
        feedbackAudio.PlayOneShot(failureAudioClip);
        PlayableDirectorFeedback(invalidWordFeedback);
        wordGameInput.SetInputActive(false);
        DoFeedbackTimeout(INVALID_FEEDBACK_TIMEOUT, FeedbackTimeout);
    }

    private void PlayableDirectorFeedback(PlayableDirector playableDirector)
    {
        playableDirector.time = 0;
        playableDirector.Stop();
        playableDirector.Evaluate();
        playableDirector.Play();
    }

    private void FeedbackTimeout()
    {
        CurrentWord.ClearAllLetters();
        _text = string.Empty;
        wordGameInput.SetInputActive(true);
    }

    private void HandleAnswer(bool correct)
    {
        if (correct)
        {
            CurrentWord.DoCorrectFeedbackRoutine();
            feedbackAudio.PlayOneShot(successAudioClip);
            HandleGameComplete();
        }
        else
        {
            CurrentWord.InCorrectFeedback();
            feedbackAudio.PlayOneShot(failureAudioClip);
        }

        _text = string.Empty;
        _entryIndex += 1;
        if (_entryIndex >= _words.Count)
        {
            HandleGameComplete();
        }
    }

    private void HandleGameComplete()
    {
        _completionPercent = (_wordGameData.tries - _entryIndex) / (float) _wordGameData.tries;
        DisplayScore(_completionPercent);
        _isPlaying = false;
        DoFeedbackTimeout(GAME_COMPLETE_FEEDBACK_TIMEOUT, GameFinished);
    }

    private void GameFinished()
    {
        _onGameFinished?.Invoke(_completionPercent);
    }

    private void ValueChanged(string text)
    {
        if (!CanPlay || (_text + text).Length > _wordLength)
            return;
        _text += text;
        CurrentWord.AddLetter(_text.Length - 1, _text.Last());
        if(_text.Length == _wordLength)
        {
            Submit();
        }
    }

    private void RemoveLastLetter()
    {
        if (_text.Length - 1 < 0)
            return;
        CurrentWord.ClearLetter(_text.Length - 1);
        _text = _text.Substring(0, _text.Length - 1);
    }
    
    private void DoFeedbackTimeout(float time, Action onFeedbackTimeout)
    {
        _feedBackTimeoutRoutine = FeedbackTimeOutRoutine(time, onFeedbackTimeout);
        StartCoroutine(_feedBackTimeoutRoutine);
    }

    private IEnumerator FeedbackTimeOutRoutine(float time, Action onFeedbackTimeout)
    {
        yield return new WaitForSeconds(time);
        onFeedbackTimeout?.Invoke();
    }
}