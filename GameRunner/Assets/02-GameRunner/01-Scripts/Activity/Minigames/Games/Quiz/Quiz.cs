using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine;
using TMPro;

using Cohort.Patterns;

namespace Cohort.GameRunner.Minigames.Quiz {
	public class Quiz : Minigame {

		public override float Score {
			get { return _score;}
			set { _score = value; }
		}

		private float _score;
		
		public UnityEvent onCorrect;
		public UnityEvent onIncorrect;
		[SerializeField] private float _graphicTimeout = 2f;
		
		private QuizData _data;
		private int _questionIndex = 0;
		private int _correctCount;

		private ObjectPool<string, QuizAnswer> _pool;
		[SerializeField] private CanvasGroup _answerGroup;
		[SerializeField] private QuizAnswer _template;
		[SerializeField] private TMP_Text _questionField;
		
		public override void Initialize(string gameData, int scoreMultiplier, Action<float> onFinished, Action onExit) {
			base.Initialize(gameData, scoreMultiplier, onFinished, onExit);
			
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
				StartCoroutine(GraphicTimeout());
			}
			else {
				//end screen routine
				StartCoroutine(OnQuizFinished());
			}
		}

		private IEnumerator GraphicTimeout() {
			_answerGroup.interactable = false;
			yield return new WaitForSeconds(_graphicTimeout);
			_answerGroup.interactable = true;
			ShowQuestion();
		}

		private void ShowQuestion() {
			_questionField.text = _data._questions[_questionIndex].question;
			_pool.SetAll(_data._questions[_questionIndex].answers);
		}

		private IEnumerator OnQuizFinished() {
			yield return new WaitForSeconds(_graphicTimeout);

			FinishMinigame();
		}
	}
}