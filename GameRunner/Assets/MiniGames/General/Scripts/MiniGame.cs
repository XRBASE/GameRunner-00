using System;
using System.Collections;
using TMPro;
using UnityEngine;

public abstract class MiniGame : Learning
{
    public ScoreUI scoreUI;
    [SerializeField] protected TextMeshProUGUI _title;
    protected Action<float> _onGameFinished;
    protected float _scoreMultiplier;
    protected float _completionPercent;
    private IEnumerator _feedBackTimeoutRoutine;

    protected abstract void BuildGame();

    protected virtual void FinishGame()
    {
        _onGameFinished?.Invoke(_completionPercent);
    }

    protected abstract void CorrectFeedback();

    protected abstract void IncorrectFeedback();

    protected abstract void GameFinishedFeedback();

    protected void DoFeedbackTimeout(float time, Action onFeedbackTimeout)
    {
        if (_feedBackTimeoutRoutine != null)
        {
            StopCoroutine(_feedBackTimeoutRoutine);
        }

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