using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class InteractiveVideoFeedback : MonoBehaviour
{
    [SerializeField] private PlayableDirector _playableDirector;

    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;
    
    private Color _correctColor = Color.green;
    private Color _incorrectColor = Color.red;
    private Color _timeOutColor = Color.yellow;
    public enum FeedBackState
    {
        Correct,
        Incorrect,
        TimeOut
    };


    private void StartPlayable()
    {
        _playableDirector.time = 0;
        _playableDirector.Stop();
        _playableDirector.Evaluate();
        _playableDirector.Play();
    }

    public void PlayFeedback(string text, FeedBackState state)
    {
        SetText(text);
        switch (state)
        {
            case FeedBackState.Correct:
                _textMeshProUGUI.color = _correctColor;
                break;
            case FeedBackState.Incorrect:
                _textMeshProUGUI.color = _incorrectColor;
                break;
            case FeedBackState.TimeOut:
                _textMeshProUGUI.color = _timeOutColor;
                break;
        }
        StartPlayable();
    }

    private void SetText(string text)
    {
        _textMeshProUGUI.text = text;
    }
}