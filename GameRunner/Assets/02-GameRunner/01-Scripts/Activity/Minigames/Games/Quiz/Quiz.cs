using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

using Cohort.Patterns;

namespace Cohort.GameRunner.Minigames.Quiz {
	public class Quiz : Minigame {
		protected override float CorrectVisualDuration {
			get { return 2f; }
		}
		protected override float FaultiveVisualDuration {
			get { return 2f; }
		}
		protected override float FinishedVisualDuration {
			get { return 2f; }
		}

		public override float Score {
			get { return _score;}
			set { _score = value; }
		}

		private float _score;
		
		public UnityEvent onCorrect;
		public UnityEvent onIncorrect;
		
		private QuizData _data;
		private int _questionIndex = 0;
		private int _correctCount;

		private ObjectPool<string, QuizAnswer> _pool;
		[SerializeField] private CanvasGroup _answerGroup;
		[SerializeField] private QuizAnswer _template;
		[SerializeField] private TMP_Text _questionField;
		
		public override void Initialize(string gameData, float timeLimit, Action<FinishCause, float> onFinished, Action onExit) {
			base.Initialize(gameData, timeLimit, onFinished, onExit);
			
			_data = JsonUtility.FromJson<QuizData>(gameData);

			_template.onAnswerGiven = OnAnswerChanged;
			_pool = new ObjectPool<string, QuizAnswer>(_template);

			ShowQuestion();
		}

		public void OnAnswerChanged(int index) {
			if (_questionIndex < 0 || _questionIndex >= _data._questions.Length) {
				return;
			}
			
			if (index == _data._questions[_questionIndex].correctAnswerIndex) {
				_correctCount++;
				_score = (float) _correctCount / _data._questions.Length;

				onCorrect?.Invoke();
			}
			else {
				onIncorrect?.Invoke();
			}

			for (int i = 0; i < _pool.Active.Count; i++) {
				_pool.Active[i].OnSubmit(i == _data._questions[_questionIndex].correctAnswerIndex);
			}

			_questionIndex++;
			if (_questionIndex < _data._questions.Length) {
				DisableAnswerInteraction();
				StartCoroutine(DoTimeout(CorrectVisualDuration, (Action)EnableAnswerInteraction + ShowQuestion));
			}
			else {
				//end screen routine
				StartCoroutine(OnQuizFinished());
			}
		}

		private void DisableAnswerInteraction() {
			_answerGroup.interactable = false;
		}

		private void EnableAnswerInteraction() {
			_answerGroup.interactable = true;
		}

		private void ShowQuestion() {
			_questionField.text = _data._questions[_questionIndex].question;
			_pool.SetAll(_data._questions[_questionIndex].answers);
		}

		private IEnumerator OnQuizFinished() {
			yield return DoTimeout(FinishedVisualDuration, null);
			FinishMinigame();
		}
	}
}