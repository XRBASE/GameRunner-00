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
    public override float Score { get; set; }
    

    public MatchGameLibrarySO matchGameLibrarySo;
    public MatchElement matchElementPrefab;

    public FollowCursor followCursor;
    public Transform answerElementParent, questionElementParent, dragTransform;
    public AudioSource pitchedAudio;
    public AudioClip gameCompleteAudioClip;
    public AudioClip correctSoundEffect;
    public AudioClip inCorrectSoundEffect;
    public AudioClip selectSoundEffect;
    public AudioClip matchFoundSoundEffect;

    public TMP_Text title;
    public AudioSource feedbackAudio;

    private MatchGameData _matchGameData;
    private MatchElement _dragElement;
    private EventSystem _eventSystem;
    private List<MatchPair> _matchPairs;
    private static Random _rng = new Random();
    private List<MatchPairData> _matches;
    private MatchElement _questionElement;
    private MatchElement _answerElement;
    private int _pairAmount;
    private int _attempts;
    private int _foundMatches;

    public override void Initialize(string gameData, float timeLimit, Action<FinishCause, float> onGameFinished,
        Action onExit)
    {
        base.Initialize(gameData, timeLimit, onGameFinished, onExit);
        _matchGameData = JsonUtility.FromJson<MatchGameData>(gameData);
        _pairAmount = _matchGameData.chosenIds.Count;
        followCursor.Initialise((RectTransform)transform);
        title.text = _matchGameData.title;
        BuildGame();
        
    }

    private void BuildGame()
    {
        _matchPairs = new List<MatchPair>();
        _matches = new List<MatchPairData>();
        foreach (var id in _matchGameData.chosenIds)
        {
            _matches.Add(matchGameLibrarySo.MatchGameLibrary.First(item => item.UID == id));
        }

        for (int i = 0; i < _pairAmount && _matches.Count > 0; i++)
        {
            var match = _matches[UnityEngine.Random.Range(0, _matches.Count)];
            _matches.Remove(match);
            
            var answerElement = Instantiate(matchElementPrefab, answerElementParent);
            answerElement.Initialise(match.originMatchType, match.originSprite, match.originText);
            answerElement.onMatchSelected = SetSelectedAnswer;
            
            var questionElement = Instantiate(matchElementPrefab, questionElementParent);
            questionElement.Initialise(match.targetMatchType, match.targetSprite, match.targetText);
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
        Score = (float) _pairAmount / _attempts;
        
        feedbackAudio.PlayOneShot(gameCompleteAudioClip);
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
        if (matchElement != null)
        {
            matchElement.Deselect();
            if (matchElement == newMatchElement)
            {
                if(_dragElement!= null)
                    Destroy(_dragElement.gameObject);
                matchElement = null;
                return;
            }
 
        }
        matchElement = newMatchElement;
        if(_dragElement!= null)
            Destroy(_dragElement.gameObject);
        if(_questionElement == null || _answerElement == null)
            CopyToDragElement(newMatchElement);
        matchElement.Select();
        Submit();
    }

    private void CopyToDragElement(MatchElement element)
    {
        _dragElement = Instantiate(element,dragTransform);
        ((RectTransform) _dragElement.transform).sizeDelta = new Vector2(120, 120);
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
        InputManager.Instance.SetActionMapActive(InputManager.ActionMaps.UI, true);
        if(_dragElement!= null)
            Destroy(_dragElement.gameObject);
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