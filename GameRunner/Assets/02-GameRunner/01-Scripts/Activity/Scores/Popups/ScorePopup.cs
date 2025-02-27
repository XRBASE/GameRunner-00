using Cohort.Patterns;
using TMPro;
using UnityEngine;

namespace Cohort.GameRunner.Score {
    public class ScorePopup : MonoBehaviour, ObjectPool<int, ScorePopup>.IPoolable {
        private static string POINTS_KEYWORD = "[POINTS]";
        private static string TEMPLATE_TEXT = "";

        public bool IsActive {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }

        [SerializeField] private float _animHeight = 100f;
        [SerializeField] private float _duration = 2f;
        [SerializeField] private AnimationCurve _posCurve;
        [SerializeField] private AnimationCurve _alphaCurve;

        [SerializeField] private TMP_Text _field;
        [SerializeField] private CanvasGroup _canvas;

        private bool _inAnimation;
        private float timer;
        private Vector2 _start, _stop;

        private ObjectPool<int, ScorePopup> _pool;
        private RectTransform _rt;

        public void Initialize(ObjectPool<int, ScorePopup> pool) {
            if (string.IsNullOrEmpty(TEMPLATE_TEXT)) {
                TEMPLATE_TEXT = _field.text;
            }

            _pool = pool;
            _rt = (RectTransform)transform;

            _start = _rt.anchoredPosition;
            _stop = _rt.anchoredPosition + new Vector2(0f, _animHeight);
        }

        private void Update() {
            if (!_inAnimation) {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= _duration) {
                timer = _duration;

                _rt.anchoredPosition = _stop;
                _canvas.alpha = 0f;

                _inAnimation = false;
                _pool.RemoveItem(this);
            }
            else {
                _rt.anchoredPosition = Vector2.Lerp(_start, _stop, _posCurve.Evaluate(timer / _duration));
                _canvas.alpha = _alphaCurve.Evaluate(timer / _duration);
            }
        }

        public void UpdatePoolable(int index, int data) {
            _field.text = GetPointstring(data);

            _rt.anchoredPosition = _start;
            _canvas.alpha = 1f;

            _inAnimation = true;
            timer = 0f;
        }

        private string GetPointstring(int points) {
            return TEMPLATE_TEXT.Replace(POINTS_KEYWORD, points.ToString());
        }

        public ScorePopup Copy() {
            return Instantiate(this, transform.parent);
        }
    }
}