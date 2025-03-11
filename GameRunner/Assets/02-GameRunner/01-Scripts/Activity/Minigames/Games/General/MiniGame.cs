using System;
using System.Collections;
using Cohort.GameRunner.Minigames;
using TMPro;
using UnityEngine;

public abstract class MiniGame : Minigame
{
    protected const float GAME_COMPLETE_FEEDBACK_TIMEOUT = 2f;
    protected const float INCORRECT_FEEDBACK_TIMEOUT = .5f;
    public ScoreUI scoreUI;
    [SerializeField] protected TextMeshProUGUI _title;
    public AudioSource feedbackAudio;
    protected float _scoreMultiplier;
    
    private IEnumerator _feedBackTimeoutRoutine;

    protected abstract void BuildGame();

    protected abstract void CorrectFeedback();

    protected abstract void IncorrectFeedback();

    protected abstract void GameFinishedFeedback();

    protected void DoFeedbackTimeout(float time, Action onFeedbackTimeout)
    {
        _feedBackTimeoutRoutine = FeedbackTimeOutRoutine(time, onFeedbackTimeout);
        StartCoroutine(_feedBackTimeoutRoutine);
    }

    // Coroutine to handle the timeout period
    private IEnumerator FeedbackTimeOutRoutine(float time, Action onFeedbackTimeout)
    {
        yield return new WaitForSeconds(time);
        onFeedbackTimeout?.Invoke();
    }
}