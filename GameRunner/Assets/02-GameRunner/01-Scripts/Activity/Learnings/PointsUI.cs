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
    [SerializeField] private ParticleSystem _confetti;
    
    [SerializeField] private GameObject _successVisuals;
    [SerializeField] private GameObject _failureVisuals;
    [SerializeField] private TMP_Text[] _fields;

    private string[] _fieldTemplates;
    private bool _animate;
    private float _timer;
    private float _score;

    private void Awake() {
        UILocator.Register(this);
        
        _fieldTemplates = new string[_fields.Length];
        for (int i = 0; i < _fields.Length; i++) {
            _fieldTemplates[i] = _fields[i].text;
        }
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
        bool success = score >= LearningManager.FAILURE_THRESHOLD;
        bool amazing = score >= LearningManager.AMAZING_THRESHOLD;
        
        _successVisuals.gameObject.SetActive(success);
        _failureVisuals.gameObject.SetActive(!success);
        
        for (int i = 0; i < _fields.Length; i++) {
            _fields[i].text = GetInfoString(Mathf.RoundToInt(score * scoreMod), learningName, _fieldTemplates[i]);
        }
        
        _score = score;
        if (amazing) {
            _confetti.Clear();
            _confetti.Play();
        }
        
        AnimateSlider();
    }

    private string GetInfoString(int score, string learning, string template) {
        if (template.Contains(POINTS_KEYWORD)) {
            template = template.Replace(POINTS_KEYWORD, score.ToString());
        }
        if (template.Contains(NAME_KEYWORD)) {
            template = template.Replace(NAME_KEYWORD, learning);
        }

        return template;
    }

    private void AnimateSlider() {
        _timer = 0f;
        _animate = true;
    }
}
