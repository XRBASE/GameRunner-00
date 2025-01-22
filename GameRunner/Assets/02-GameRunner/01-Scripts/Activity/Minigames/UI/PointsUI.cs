using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Cohort.UI.Generic;

namespace Cohort.GameRunner.Minigames {
    public class PointsUI : UIPanel {
        private const string POINTS_KEYWORD = "[POINTS]";
        private const string NAME_KEYWORD = "[MINIGAME]";

        private readonly Dictionary<Minigame.FinishCause, string> BODIES =
            new Dictionary<Minigame.FinishCause, string> {
                {Minigame.FinishCause.Completed, "You finished [MINIGAME] and earned [POINTS] points!"},
                {Minigame.FinishCause.Failed, "You failed [MINIGAME]... \n[POINTS] Points earned."},
                {Minigame.FinishCause.Timeout, "You ran out of time... \n[POINTS] Points earned."},
                {Minigame.FinishCause.ActivityStop, "The activity was stopped. \n[POINTS] Points earned!"}
            };
        private readonly Dictionary<Minigame.FinishCause, string> TITLES =
            new Dictionary<Minigame.FinishCause, string> {
                {Minigame.FinishCause.Completed, "[MINIGAME] minigame finished!"},
                {Minigame.FinishCause.Failed, "[MINIGAME] minigame failed!"},
                {Minigame.FinishCause.Timeout, "Time limit reached!"},
                {Minigame.FinishCause.ActivityStop, "Activity stopped!"}
            };

        [SerializeField] private Slider _scoreSlider;
        [SerializeField] private float _slideDuration;
        [SerializeField] private AnimationCurve _scoreSlideCurve;
        [SerializeField] private ParticleSystem _confetti;
        
        [SerializeField] private TMP_Text _bodyField;
        [SerializeField] private TMP_Text _titleField;
        
        private bool _animate;
        private float _timer;
        private float _score;

        private void Awake() {
            UILocator.Register(this);
        }

        private void Start() {
            MinigameManager.Instance.onMinigameFinished += OnMinigameFinished;

            Deactivate();
        }

        private void OnDestroy() {
            UILocator.Remove<PointsUI>();
            MinigameManager.Instance.onMinigameFinished -= OnMinigameFinished;
        }

        private void OnMinigameFinished(Minigame.FinishCause cause, float score) {
            Activate();
            
            ShowScore(score, MinigameManager.Instance.ScoreMultiplier,
                      MinigameManager.Instance.Current.actionDescription, cause);
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
            _scoreSlider.value = _scoreSlideCurve.Evaluate(Mathf.Clamp01(_timer / _slideDuration)) * _score;
        }

        private void ShowScore(float score, int scoreMod, string learningName, Minigame.FinishCause cause) {
            bool amazing = score >= MinigameManager.AMAZING_THRESHOLD;

            _bodyField.text = GetInfoString(Mathf.RoundToInt(score * scoreMod), learningName, BODIES[cause]);
            _titleField.text = GetInfoString(Mathf.RoundToInt(score * scoreMod), learningName, TITLES[cause]);

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
}