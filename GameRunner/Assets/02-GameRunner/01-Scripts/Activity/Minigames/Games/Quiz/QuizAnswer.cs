using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

using Cohort.Patterns;

namespace Cohort.GameRunner.Minigames.Quiz {
    public class QuizAnswer : MonoBehaviour, ObjectPool<string, QuizAnswer>.IPoolable {
        public bool IsActive {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }

        public Action<int> onAnswerGiven;

        public UnityEvent onSubmitCorrect;
        public UnityEvent onSubmitIncorrect;
        public UnityEvent onReset;

        [SerializeField] private TMP_Text _field;
        [SerializeField] private Button _control;

        private int _index;
        private ObjectPool<string, QuizAnswer> _pool;

        public void Initialize(ObjectPool<string, QuizAnswer> pool) {
            _control.onClick.AddListener(OnClick);
        }

        private void OnDestroy() {
            onAnswerGiven = null;
        }

        public void UpdatePoolable(int index, string data) {
            _field.text = data;
            _index = index;

            onReset?.Invoke();
        }

        private void OnClick() {
            onAnswerGiven?.Invoke(_index);
        }

        public void OnSubmit(bool thisAnswerWasCorrect) {
            if (thisAnswerWasCorrect) {
                onSubmitCorrect?.Invoke();
            }
            else {
                onSubmitIncorrect?.Invoke();
            }
        }

        public QuizAnswer Copy() {
            QuizAnswer a = Instantiate(this, transform.parent);
            a.onAnswerGiven = onAnswerGiven;
            //a.GetComponent<QuizBtnVisuals>().enabled = false;

            return a;
        }
    }
}
