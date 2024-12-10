using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class MatchGame : MiniGame
{
    public MatchGameDataSO MatchGameDataSo;
    public MatchElement matchElementPrefab;
    public Transform answerElementParent, questionElementParent;
    private List<MatchPair> _matchPairs;
    private static Random _rng = new Random();
    private MatchElement _questionElement;
    private MatchElement _answerElement;
    private MatchGameData _matchGameData;
    public List<MatchPairData> matches;
    

    public override void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished)
    {
        _matchGameData = JsonUtility.FromJson<MatchGameData>(gameData);
        _onGameFinished = onGameFinished;
        _scoreMultiplier = scoreMultiplier;
        _title.text = _matchGameData.title;
        BuildGame();
    }

    protected override void Awake()
    {
        //base.Awake();
        Initialize(JsonUtility.ToJson(MatchGameDataSo.matchGameData), 1, delegate(float f) {Debug.LogError(f); });
    }

    protected override void BuildGame()
    {
        _matchPairs = new List<MatchPair>();
        matches = _matchGameData.matches;
        for (int i = 0; i < _matchGameData.pairAmount; i++)
        {
            var match = matches[UnityEngine.Random.Range(0, matches.Count)];
            matches.Remove(match);
            var answerElement = Instantiate(matchElementPrefab, answerElementParent);
            if (match.originMatchType == MatchPairData.MatchType.Text)
            {
                answerElement.SetPreviewElement(match.originText);
            } else if (match.originMatchType == MatchPairData.MatchType.Image)
            {
                answerElement.SetPreviewElement(match.originSprite);
            }
            answerElement.onMatchSelected += SetSelectedAnswer;
            var questionElement = Instantiate(matchElementPrefab, questionElementParent);
            
            if (match.targetMatchType == MatchPairData.MatchType.Text)
            {
                questionElement.SetPreviewElement(match.targetText);
            } else if (match.targetMatchType == MatchPairData.MatchType.Image)
            {
                questionElement.SetPreviewElement(match.targetSprite);
            }
            questionElement.onMatchSelected += SetSelectedQuestion;
            var matchPair = new MatchPair(i, answerElement, questionElement);
            _matchPairs.Add(matchPair);
        }

        ShuffleMatches();
    }

    protected override void FinishGame()
    {
        throw new NotImplementedException();
    }

    protected override void CorrectFeedback()
    {
        Debug.LogError("Correct");
    }

    protected override void IncorrectFeedback()
    {
        Debug.LogError("InCorrect");
    }

    protected override void GameFinishedFeedback()
    {
        throw new NotImplementedException();
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
        HandleSelect(_answerElement, answerElement);
        _answerElement = answerElement;
        Submit();
    }

    private void SetSelectedQuestion(MatchElement questionElement)
    {
        HandleSelect(_questionElement, questionElement);
        _questionElement = questionElement;
        Submit();
    }

    private void HandleSelect(MatchElement oldMatchElement,MatchElement matchElement)
    {
        //Deselect old answer
        if (oldMatchElement != null)
            oldMatchElement.SetSelectState(false);
        matchElement.SetSelectState(true);
    }
    

    public void Submit()
    {
        if(_answerElement == null || _questionElement == null)
            return;
        if (_answerElement.id == _questionElement.id)
        {
            CorrectFeedback();
        }
        else
        {
            IncorrectFeedback();
        }
        
        _questionElement.SetSelectState(false);
        _answerElement.SetSelectState(false);
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

    // Display the score after each puzzle or at the end
    private void DisplayScore(float score)
    {
        scoreUI.PlayScore((int) (score * _scoreMultiplier));
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