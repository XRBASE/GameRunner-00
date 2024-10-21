using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordGame : Minigame
{
    private Word _currentWord => _words[_entryIndex];
    private bool CanPlay => IsPlaying && _entryIndex < _words.Count;
    public WordGameInput wordGameInput;
    public Transform gameParent;
    public TextMeshProUGUI title;
    public Word wordPrefab;
    public AudioSource feedbackAudio;
    public AudioClip successAudioClip, failureAudioClip;
    public ScoreUI scoreUI;
    public TextMeshProUGUI hintText;
    private int _wordLength;

    private List<Word> _words = new List<Word>();
    private WordGameData _wordGameData;
    private Action<float> _onGameFinished;
    private WordData _chosenWord;
    private int _entryIndex;
    private string _text = string.Empty;
    private List<string> _wordDictionary;
    
    private float _completionPercent;
    private float _scoreMultiplier;
    private Action<float> _testOnGameFinished;
    public WordGameDataSO _wordGameDataSo;

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

    private void TestData()
    {
        _testOnGameFinished += delegate(float f) {  Debug.LogError($"Score: {f}");};
        Initialize(_wordGameDataSo.wordGameDataJson(), 1, _testOnGameFinished);
    }
    
    protected override void Awake()
    {
        //base.Awake();
        wordGameInput.onKeyboardInput += ValueChanged;
        wordGameInput.remove += RemoveLastLetter;
        TestData();
    }

    private void GameFinished(float score)
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
        Debug.LogError(_chosenWord.word);
        _entryIndex = 0;
        IsPlaying = true;
    }

    private void Submit()
    {
        if (!CanPlay || _text.Length < _wordLength)
            return;
        if (!_wordDictionary.Contains(_text))
        {
            Debug.LogError("Word does not contain in word list");
            _text = string.Empty;
            _currentWord.ClearAllLetters();
            _currentWord.InCorrectFeedback();
            feedbackAudio.PlayOneShot(failureAudioClip);
            return;
        }

        HandleAnswer(_currentWord.CheckWord(_chosenWord.word));
    }

    private void HandleAnswer(bool correct)
    {
        if (correct)
        {
            _currentWord.DoCorrectFeedbackRoutine();
            feedbackAudio.PlayOneShot(successAudioClip);
            HandleGameComplete();
        }
        else
        {
            _currentWord.InCorrectFeedback();
            feedbackAudio.PlayOneShot(failureAudioClip);
        }

        _text = string.Empty;
        _entryIndex += 1;
        if (_entryIndex > _words.Count)
        {
            HandleGameComplete();
        }
    }

    private void HandleGameComplete()
    {
        _completionPercent = (_wordGameData.tries - _entryIndex) / (float) _wordGameData.tries;
        GameFinished(_completionPercent);
        IsPlaying = false;
        DoDelayedGameComplete();
    }

    private void ValueChanged(string text)
    {
        if (!CanPlay || (_text + text).Length > _wordLength)
            return;
        _text += text;
        _currentWord.AddLetter(_text.Length - 1, _text.Last());
        if(_text.Length == _wordLength)
        {
            Submit();
        }
    }

    private void RemoveLastLetter()
    {
        if (_text.Length - 1 < 0)
            return;
        _currentWord.ClearLetter(_text.Length - 1);
        _text = _text.Substring(0, _text.Length - 1);
    }

    private void DoDelayedGameComplete()
    {
        StartCoroutine(DelayedGameCompleteRoutine());
    }

    private IEnumerator DelayedGameCompleteRoutine()
    {
        yield return new WaitForSeconds(2f);
        _onGameFinished?.Invoke(_completionPercent);
        
    }
}