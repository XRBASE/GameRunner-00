using System;
using System.Collections;
using Cohort.Patterns;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Quiz : Learning {

	public UnityEvent onCorrect;
	public UnityEvent onIncorrect;
	[SerializeField] private float _graphicTimeout = 2f;
	
	private Action<float> _onLearningFinished;
	
	private QuizData _data;
	private int _scoreMultiplier;
	private int _questionIndex = 0;
	private int _correctCount;

	private ObjectPool<string, QuizAnswer> _pool;
	[SerializeField] private CanvasGroup _answerGroup;
	[SerializeField] private QuizAnswer _template;
	[SerializeField] private TMP_Text _questionField;
	
	public override void Initialize(string gameData, int scoreMultiplier, Action<float> onLearningFinished) {
		_data = JsonUtility.FromJson<QuizData>(gameData);
		_scoreMultiplier = scoreMultiplier;
		_onLearningFinished = onLearningFinished;

		_template.onAnswerGiven = OnAnswerChanged;
		_pool = new ObjectPool<string, QuizAnswer>(_template);
		
		ShowQuestion();
	}

	public void OnAnswerChanged(int index) {
		if (index == _data._questions[_questionIndex].correctAnswerIndex) {
			_correctCount++;
			
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
		
		_onLearningFinished?.Invoke((float)_correctCount / _data._questions.Length);
	}

	public override void StopLearning() {
		_onLearningFinished?.Invoke((float)_correctCount / _data._questions.Length);
	}
}
