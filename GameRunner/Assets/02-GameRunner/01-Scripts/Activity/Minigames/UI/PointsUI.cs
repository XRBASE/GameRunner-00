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
                {Minigame.FinishCause.FinPerfect, "Amazing, you finished [MINIGAME] perfectly and earned [POINTS] points!"},
                {Minigame.FinishCause.FinSuccess, "You finished [MINIGAME] and earned [POINTS] points!"},
                {Minigame.FinishCause.FinFailed, "You failed [MINIGAME]... \n[POINTS] Points earned."},
                {Minigame.FinishCause.Timeout, "You ran out of time... \n[POINTS] Points earned."},
                {Minigame.FinishCause.ActivityStop, "The activity was stopped. \n[POINTS] Points earned!"}
            };
        private readonly Dictionary<Minigame.FinishCause, string> TITLES =
            new Dictionary<Minigame.FinishCause, string> {
                {Minigame.FinishCause.FinPerfect, "[MINIGAME] perfect score!"},
                {Minigame.FinishCause.FinSuccess, "[MINIGAME] minigame finished!"},
                {Minigame.FinishCause.FinFailed, "[MINIGAME] minigame failed!"},
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

        private void OnMinigameFinished(Minigame.FinishCause cause, int score) {
            if (cause == Minigame.FinishCause.FinPointless)
                return;
            
            Activate();
            ShowScore(score, MinigameManager.Instance.Current.minScore, MinigameManager.Instance.Current.maxScore,
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

        private void ShowScore(int score, int minScore, int maxscore, string learningName, Minigame.FinishCause cause) {
            _bodyField.text = GetInfoString(score, learningName, BODIES[cause]);
            _titleField.text = GetInfoString(score, learningName, TITLES[cause]);

            _score = (float)(score - minScore) / (maxscore - minScore);
            if (cause == Minigame.FinishCause.FinPerfect) {
                if (_confetti == null) {
                    //prevents error breaking and the panel not closing
                    Debug.LogError("Missing reference to confetti particles");
                }
                else {
                    _confetti.Clear();
                    _confetti.Play();
                }
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