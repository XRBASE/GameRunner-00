using System;
using System.Collections;
using System.Collections.Generic;
using Cohort.GameRunner.Input;
using Cohort.GameRunner.Minigames;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

public class MatchGame : Minigame
{
    protected override float CorrectVisualDuration {
        get { return 1f; }
    }
    protected override float FaultiveVisualDuration {
        get { return 0.5f; }
    }
    protected override float FinishedVisualDuration {
        get { return 2f; }
    }
    public override int Score { get; set; }
    
    public MatchGameDataSO MatchGameDataSo;
    public MatchElement matchElementPrefab;
    public Transform answerElementParent, questionElementParent;
    public AudioSource pitchedAudio;
    public AudioClip gameCompleteAudioClip;
    public AudioClip correctSoundEffect;
    public AudioClip inCorrectSoundEffect;
    public AudioClip selectSoundEffect;
    public AudioClip matchFoundSoundEffect;

    public TMP_Text title;
    public AudioSource feedbackAudio;

    private EventSystem _eventSystem;
    private List<MatchPair> _matchPairs;
    private static Random _rng = new Random();
    private List<MatchPairData> _matches;
    private MatchElement _questionElement;
    private MatchElement _answerElement;
    private MatchGameData _matchGameData;
    private int _attempts;
    private int _foundMatches;
    
    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore, Action<FinishCause, int> onFinished, Action onExit)
    {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);
        
        _matchGameData = MatchGameDataSo.matchGameData;
        title.text = _matchGameData.title;
        BuildGame();
    }

    private void BuildGame()
    {
        _matchPairs = new List<MatchPair>();
        _matches = new List<MatchPairData>();
        _matches.AddRange(_matchGameData.matches);
        
        for (int i = 0; i < _matchGameData.pairAmount && _matches.Count > 0; i++)
        {
            var match = _matches[UnityEngine.Random.Range(0, _matches.Count)];
            _matches.Remove(match);
            var answerElement = Instantiate(matchElementPrefab, answerElementParent);
            if (match.originMatchType == MatchPairData.MatchType.Text)
            {
                answerElement.SetPreviewElement(match.originText);
            }
            else if (match.originMatchType == MatchPairData.MatchType.Image)
            {
                answerElement.SetPreviewElement(match.originSprite);
            }

            answerElement.onMatchSelected = SetSelectedAnswer;
            var questionElement = Instantiate(matchElementPrefab, questionElementParent);
            if (match.targetMatchType == MatchPairData.MatchType.Text)
            {
                questionElement.SetPreviewElement(match.targetText);
            }
            else if (match.targetMatchType == MatchPairData.MatchType.Image)
            {
                questionElement.SetPreviewElement(match.targetSprite);
            }

            questionElement.onMatchSelected = SetSelectedQuestion;
            var matchPair = new MatchPair(i, answerElement, questionElement);
            _matchPairs.Add(matchPair);
        }

        ShuffleMatches();
    }


    private void CorrectFeedback()
    {
        _answerElement.Complete();
        _questionElement.Complete();
        feedbackAudio.PlayOneShot(correctSoundEffect);
    }

    private void IncorrectFeedback()
    {
        _questionElement.WrongAnswer();
        _answerElement.WrongAnswer();
        feedbackAudio.PlayOneShot(inCorrectSoundEffect);
        
        InputManager.Instance.SetActionMapActive(InputManager.ActionMaps.UI, false);

        StartCoroutine(DoTimeout(FaultiveVisualDuration, Deselect));
    }

    private void GameFinishedFeedback()
    {
        Score = _scoreRange.GetValueRound((float) _matchGameData.pairAmount / _attempts, true);
        
        feedbackAudio.PlayOneShot(gameCompleteAudioClip);
        DoGameFinishedFeedback();
        StartCoroutine(DoTimeout(FinishedVisualDuration, FinishMinigame));
    }

    private void ShuffleMatches()
    {
        List<int> places = new List<int>();
        for (int i = 0; i < _matchGameData.pairAmount; i++)
        {
            places.Add(i);
        }

        Shuffle(places);
        for (int i = 0; i < _matchPairs.Count; i++)
        {
            _matchPairs[i].answerElement.transform.SetSiblingIndex(places[i]);
        }

        Shuffle(places);
        for (int i = 0; i < _matchPairs.Count; i++)
        {
            _matchPairs[i].questionElement.transform.SetSiblingIndex(places[i]);
        }
    }

    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void SetSelectedAnswer(MatchElement answerElement)
    {
        HandleSelect(ref _answerElement, answerElement);
    }

    private void SetSelectedQuestion(MatchElement questionElement)
    {
        HandleSelect(ref _questionElement, questionElement);
    }

    private void HandleSelect(ref MatchElement matchElement, MatchElement newMatchElement)
    {
        feedbackAudio.PlayOneShot(selectSoundEffect);
        //Deselect old answer
        if (matchElement != null && matchElement != newMatchElement)
            matchElement.Deselect();
        matchElement = newMatchElement;
        matchElement.Select();
        Submit();
    }

    public void Submit()
    {
        if (_answerElement == null || _questionElement == null)
            return;
        _attempts++;
        if (_answerElement.id == _questionElement.id)
        {
            _foundMatches++;
            CorrectFeedback();
            if (_foundMatches == _matchGameData.pairAmount)
            {
                GameFinishedFeedback();
            }
        }
        else
        {
            IncorrectFeedback();
            return;
        }
        Deselect();
    }

    private void Deselect()
    {
        InputManager.Instance.SetActionMapActive(InputManager.ActionMaps.UI, true);
        
        _questionElement.Deselect();
        _answerElement.Deselect();
        _questionElement = null;
        _answerElement = null;
    }

    private void Reset()
    {
        for (int i = 0; i < _matchPairs.Count; i++)
        {
            Destroy(_matchPairs[i].answerElement.gameObject);
            Destroy(_matchPairs[i].questionElement.gameObject);
        }

        _matchPairs.Clear();
    }

    private void DoGameFinishedFeedback()
    {
        StartCoroutine(GameFinishedFeedbackRoutine());
    }

    private IEnumerator GameFinishedFeedbackRoutine()
    {
        foreach (var matchPair in _matchPairs)
        {
            matchPair.answerElement.Flip();
            matchPair.questionElement.Flip();
            pitchedAudio.PlayOneShot(matchFoundSoundEffect);
            pitchedAudio.pitch += .1f;
            yield return new WaitForSeconds(.3f);
        }

        pitchedAudio.pitch = 1f;
    }
}

public class MatchPair
{
    public MatchElement answerElement;
    public MatchElement questionElement;

    public MatchPair(int id, MatchElement answerElement, MatchElement questionElement)
    {
        this.answerElement = answerElement;
        this.questionElement = questionElement;
        answerElement.id = id;
        questionElement.id = id;
        answerElement.title.text = string.Empty;
        questionElement.title.text = string.Empty;
    }
}