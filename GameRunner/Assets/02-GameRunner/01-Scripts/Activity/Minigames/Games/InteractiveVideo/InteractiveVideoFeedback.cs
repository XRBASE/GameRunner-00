using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class InteractiveVideoFeedback : MonoBehaviour
{
    [SerializeField] private PlayableDirector _playableDirector;

    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;


    private void StartPlayable()
    {
        _playableDirector.time = 0;
        _playableDirector.Stop();
        _playableDirector.Evaluate();
        _playableDirector.Play();
    }

    public void PlayFeedback(string text)
    {
        SetText(text);
        StartPlayable();
    }

    private void SetText(string text)
    {
        _textMeshProUGUI.text = text;
    }
}