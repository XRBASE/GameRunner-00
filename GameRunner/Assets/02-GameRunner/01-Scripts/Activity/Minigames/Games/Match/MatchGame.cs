using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Input;
using Cohort.GameRunner.Minigames;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = System.Random;

using AudioType = Cohort.GameRunner.Audio.Minigames.AudioType;

public class MatchGame : Minigame
{
    protected override float CorrectVisualDuration
    {
        get { return 1f; }
    }

    protected override float FaultiveVisualDuration
    {
        get { return 0.5f; }
    }

    protected override float FinishedVisualDuration
    {
        get { return 2f; }
    }
    public override int Score { get; set; }
    
    public MatchGameLibrarySO matchGameLibrarySo;
    public MatchElement matchElementPrefab;

    public Transform answerElementParent, questionElementParent, dragTransform;
    public AudioSource pitchedAudio;
    public AudioClip matchFoundSoundEffect;
    public AudioClip selectSoundEffect;

    public TMP_Text title;
    public AudioSource feedbackAudio;

    private MatchGameData _matchGameData;
    private MatchElement _dragElement;
    private EventSystem _eventSystem;
    private static Random _rng = new Random();
    private List<MatchPairData> _matches;
    private List<MatchElement> _matchElements;
    private MatchElement _selectedMatchElement;
    private MatchElement _targetMatchElement;
    private int _pairAmount;
    private int _attempts;
    private int _foundMatches;
    private bool _inClick;
    
    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore, Action<FinishCause, int> onFinished, Action onExit)
    {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);
        _matchGameData = JsonUtility.FromJson<MatchGameData>(gameData);
        _pairAmount = _matchGameData.chosenIds.Count;
        title.text = _matchGameData.title;
        BuildGame();
    }

    private void BuildGame()
    {
        _matches = new List<MatchPairData>();
        _matchElements = new List<MatchElement>();
        foreach (var id in _matchGameData.chosenIds)
        {
            _matches.Add(matchGameLibrarySo.MatchGameLibrary.First(item => item.UID == id));
        }

        for (int i = 0; i < _pairAmount && _matches.Count > 0; i++)
        {
            var match = _matches[UnityEngine.Random.Range(0, _matches.Count)];
            _matches.Remove(match);
            for (int j = 0; j < 2; j++)
            {
                MatchElement matchElement = null;
                if (j == 0)
                {
                    matchElement = Instantiate(matchElementPrefab, questionElementParent);
                    matchElement.Initialise(i, match.originMatchType, MatchElement.Category.Question,
                        match.originSprite, match.originText,
                        match.originLabelText);
                }
                else
                {
                    matchElement = Instantiate(matchElementPrefab, answerElementParent);
                    matchElement.Initialise(i, match.targetMatchType, MatchElement.Category.Answer, match.targetSprite,
                        match.targetText,
                        match.targetLabelText);
                }

                matchElement.onSelectMatch += SelectMatch;
                matchElement.onDropMatch += ReleaseMatch;
                _matchElements.Add(matchElement);
            }
        }

        ShuffleMatches();
    }


    private void CorrectFeedback(int matchId)
    {
        foreach (var matchElement in _matchElements.Where(match => match.id == matchId))
        {
            matchElement.Complete();
        }
        
        _audioHandle.PlayClip(AudioType.Success);
    }

    private void IncorrectFeedback()
    {
        _selectedMatchElement.WrongAnswer();
        _targetMatchElement.WrongAnswer();
        _audioHandle.PlayClip(AudioType.Failure);

        InputManager.Instance.SetActionMapActive(InputManager.ActionMaps.LearningCursor, false);
        StartCoroutine(DoTimeout(FaultiveVisualDuration, Deselect));
    }

    private void GameFinishedFeedback()
    {
        Score = _scoreRange.GetValueRound((float) _pairAmount / _attempts, true);
        
        DoGameFinishedFeedback();
        StartCoroutine(DoTimeout(FinishedVisualDuration, FinishMinigame));
    }

    private void ShuffleMatches()
    {
        List<int> places = new List<int>();
        for (int i = 0; i < _pairAmount; i++)
        {
            places.Add(i);
        }

        Shuffle(places);

        var answers = _matchElements.Where(match => match.category == MatchElement.Category.Answer).ToList();
        for (int i = 0; i < answers.Count; i++)
        {
            answers[i].transform.SetSiblingIndex(places[i]);
        }

        Shuffle(places);
        var questions = _matchElements.Where(match => match.category == MatchElement.Category.Question).ToList();
        for (int i = 0; i < questions.Count; i++)
        {
            questions[i].transform.SetSiblingIndex(places[i]);
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

    private void SelectMatch(MatchElement matchElement)
    {
        feedbackAudio.PlayOneShot(selectSoundEffect);
        _selectedMatchElement = matchElement;
        matchElement.Select();
        CopyToDragElement(matchElement);
    }

    private void ReleaseMatch()
    {
        if (_dragElement != null)
            Destroy(_dragElement.gameObject);
        _targetMatchElement = _matchElements.FirstOrDefault(match => match.PointerOver());
        Submit();
    }

    private void CopyToDragElement(MatchElement element)
    {
        _dragElement = Instantiate(element, dragTransform, false);
        ((RectTransform) _dragElement.transform).sizeDelta = new Vector2(120, 120);
        ((RectTransform) _dragElement.transform).anchoredPosition = Vector2.zero;
    }

    public void Submit()
    {
        if (_selectedMatchElement == null || _targetMatchElement == null ||
            _targetMatchElement == _selectedMatchElement || _selectedMatchElement.category == _targetMatchElement.category || _targetMatchElement.matchState == MatchElement.MatchState.Completed)
        {
            Deselect();
            return;
        }

        _attempts++;
        if (_selectedMatchElement.id == _targetMatchElement.id)
        {
            _foundMatches++;
            CorrectFeedback(_selectedMatchElement.id);
            if (_foundMatches == _pairAmount)
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
        InputManager.Instance.SetActionMapActive(InputManager.ActionMaps.LearningCursor, true);
        if (_dragElement != null)
            Destroy(_dragElement.gameObject);
        _selectedMatchElement?.Deselect();
        _targetMatchElement?.Deselect();
        _selectedMatchElement = null;
        _targetMatchElement = null;
    }

    private void DoGameFinishedFeedback()
    {
        StartCoroutine(GameFinishedFeedbackRoutine());
    }

    private IEnumerator GameFinishedFeedbackRoutine()
    {
        for (int i = 0; i < _pairAmount; i++)
        {
            foreach (var matchElement in _matchElements.Where(match => match.id == i))
            {
                matchElement.Flip();
            }
            
            pitchedAudio.PlayOneShot(matchFoundSoundEffect);
            pitchedAudio.pitch += .1f;
            yield return new WaitForSeconds(.3f);
        }

        pitchedAudio.pitch = 1f;
    }
}