using System;
using Cohort.UI.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointsUI : UIPanel {
    private const string POINTS_KEYWORD = "[POINTS]";
    private const string NAME_KEYWORD = "[LEARNING]";
    
    [SerializeField] private Slider _scoreSlider;
    [SerializeField] private float _slideDuration;
    [SerializeField] private AnimationCurve _scoreSlideCurve;
    [SerializeField] private TMP_Text _field;
    [SerializeField] private ParticleSystem _confetti;

    private string _templateInfo;
    private bool _animate;
    private float _timer;
    private float _score;

    private void Awake() {
        UILocator.Register(this);
        _templateInfo = _field.text;
    }

    private void Start() {
        LearningManager.Instance.onLearningFinished += OnLearningFinished;
        
        Deactivate();
    }

    private void OnDestroy() {
        UILocator.Remove<PointsUI>();
        LearningManager.Instance.onLearningFinished -= OnLearningFinished;
    }

    private void OnLearningFinished(float score) {
        Activate();
        
        ShowScore(score, LearningManager.Instance.ScoreMultiplier, LearningManager.Instance.Current.actionDescription);
    }

    public void ResetPanel() {
        _scoreSlider.value = 0f;
        _timer = 0f;
        _animate = false;
    }

    private void Update() {
        if (!_animate)
            return;

        if (_timer > _slideDuration)
            return;
        
        _timer += Time.deltaTime;
        _scoreSlider.value = _scoreSlideCurve.Evaluate( Mathf.Clamp01(_timer / _slideDuration)) * _score;
    }

    private void ShowScore(float score, int scoreMod, string learningName) {
        _score = score;
        _field.text = GetInfoString(Mathf.RoundToInt(score * scoreMod), learningName);
        _confetti.Clear();
        _confetti.Play();
        
        AnimateSlider();
    }

    private string GetInfoString(int score, string learning) {
        string info = _templateInfo;
        if (info.Contains(POINTS_KEYWORD)) {
            info = info.Replace(POINTS_KEYWORD, score.ToString());
        }
        if (info.Contains(NAME_KEYWORD)) {
            info = info.Replace(NAME_KEYWORD, learning);
        }

        return info;
    }

    private void AnimateSlider() {
        _timer = 0f;
        _animate = true;
    }
}
