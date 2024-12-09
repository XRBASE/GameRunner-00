using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class MatchGame : MiniGame
{
    public AnswerElement answerElementPrefab;
    public QuestionElement questionElementPrefab;
    public Transform answerElementParent, questionElementParent;
    private int pairAmount = 5;
    private List<MatchPair> _matchPairs;
    private static Random _rng = new Random();
    private QuestionElement _selectedQuestionElement;
    private AnswerElement _selectedAnswerElement;
    private MatchGameData _matchGameData; 

    public override void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished)
    {
        _matchGameData = JsonUtility.FromJson<MatchGameData>(gameData);
        _onGameFinished = onGameFinished;
        _scoreMultiplier = scoreMultiplier;
        _title.text = _matchGameData.title;
        BuildGame();
    }
    
    protected override void Awake() {
        //base.Awake();

    }
    
    protected override void BuildGame()
    {
        _matchPairs = new List<MatchPair>();
        for (int i = 0; i < pairAmount; i++)
        {
            var answerElement = Instantiate(answerElementPrefab, answerElementParent);
            answerElement.onMatchSelected += SetSelectedAnswer;
            var questionElement = Instantiate(questionElementPrefab, questionElementParent);
            questionElement.onMatchSelected += SetSelectedQuestion;
            var matchPair = new MatchPair(i,answerElement, questionElement);
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
        throw new NotImplementedException();
    }

    protected override void IncorrectFeedback()
    {
        throw new NotImplementedException();
    }

    protected override void GameFinishedFeedback()
    {
        throw new NotImplementedException();
    }

    private void ShuffleMatches()
    {
        List<int> places = new List<int>();
        for (int i = 0; i < pairAmount; i++)
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
        while (n > 1) {  
            n--;  
            int k = _rng.Next(n + 1);  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }

    public void SetSelectedAnswer(MatchElement answerElement)
    {
        if (_selectedAnswerElement != null)
            _selectedAnswerElement.SetSelectState(false);
        _selectedAnswerElement = (AnswerElement)answerElement;
        _selectedAnswerElement.SetSelectState(true);
    }

    public void SetSelectedQuestion(MatchElement questionElement)
    {
        if (_selectedQuestionElement != null)
            _selectedQuestionElement.SetSelectState(false);
        _selectedQuestionElement = (QuestionElement)questionElement;
        _selectedQuestionElement.SetSelectState(true);
    }

    public void Submit()
    {
        if (_selectedAnswerElement.id == _selectedQuestionElement.id)
        {
            
        }
    }
    
    // Display the score after each puzzle or at the end
    private void DisplayScore(float score)
    {
        scoreUI.PlayScore((int)(score * _scoreMultiplier));
    }
}

public class MatchPair
{
    public int id;
    public AnswerElement answerElement;
    public QuestionElement questionElement;

    public MatchPair(int id, AnswerElement answerElement, QuestionElement questionElement)
    {
        this.id = id;
        this.answerElement = answerElement;
        this.questionElement = questionElement;
        answerElement.title.text = id.ToString();
        questionElement.title.text = id.ToString();
    }
}
