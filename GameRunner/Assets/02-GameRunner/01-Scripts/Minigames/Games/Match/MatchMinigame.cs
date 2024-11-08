using System;
using UnityEngine;


public class MatchMinigame : Minigame
{
    public AnswerElement answerElementPrefab;
    public QuestionElement questionElementPrefab;

    public Transform answerElementParent, questionElementParent;

    public override void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished)
    {
        
    }
    
    protected override void Awake() {
        base.Awake();
        
        
    }
}

public class MatchPair
{
    public AnswerElement answerElement;
    public QuestionElement questionElement;

    public MatchPair(AnswerElement answerElement, QuestionElement questionElement)
    {
        answerElement = answerElement;
        questionElement = questionElement;
    }
}
