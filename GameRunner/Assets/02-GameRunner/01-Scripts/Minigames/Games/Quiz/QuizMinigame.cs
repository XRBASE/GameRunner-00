using Cohort.Patterns;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class QuizMinigame : Minigame {
    public Action<bool> onGameComplete;
    
    [SerializeField] private TMP_Text _questionField;
    [SerializeField] private QuizAnswer _answerTemplate;
    [SerializeField] private GameObject _lifeTemplate;
    [SerializeField] private Button _submitBtn;

    private GameObject[] _lifeObjects;
    private ObjectPool<string, QuizAnswer> _pool;
    private Quiz _quiz;

    private int _lives;
    private int _questionIndex = 0;
    private int _selectedAnswer = -1;
    
    public override void Initialize(string gameData, Action<bool> onGameFinished) {
        _quiz = JsonUtility.FromJson<Quiz>(gameData);
        _answerTemplate.onValueChanged += OnAnswerChanged;
        
        if (onGameComplete == null)
            onGameComplete = onGameFinished;
        else
            onGameComplete += onGameFinished;

        _questionIndex = 0;
        _submitBtn.onClick.AddListener(AnswerQuestion);
        
        _lives = _quiz.lives;
        _lifeObjects = new GameObject[_lives];
        _lifeObjects[0] = _lifeTemplate;
        for (int i = 1; i < _lives; i++) {
            _lifeObjects[i] = Instantiate(_lifeTemplate, _lifeTemplate.transform.parent);
        }

        _pool = new ObjectPool<string, QuizAnswer>(_answerTemplate);
        ShowQuestion();
    }

    public void ShowQuestion() {
        _questionField.text = _quiz[_questionIndex].question;
        _pool.SetAll(_quiz[_questionIndex].answers);

        for (int i = 0; i < _pool.Active.Count; i++) {
            _pool.Active[i].Value = false;
        }
    }

    private void OnAnswerChanged(int answerIndex, bool selected) {
        if (!selected) {
            _selectedAnswer = -1;
        }
        else {
            _selectedAnswer = answerIndex;
        }
    }

    public void AnswerQuestion() {
        bool correct = _selectedAnswer == _quiz[_questionIndex].correctId;

        if (!correct) {
            OnIncorrect();

            if (_lives <= 0)
                return;
        }
        
        _questionIndex++;
        if (_questionIndex < _quiz.Length) {
            ShowQuestion();
        }
        else {
            OnFinish(true);
        }
    }

    public void OnFinish(bool complete) {
        onGameComplete?.Invoke(complete);
    }

    public void OnIncorrect() {
        _lives--;
        
        for (int i = 0; i < _lifeObjects.Length; i++) {
            _lifeObjects[i].SetActive(i < _lives);
        }
        
        if (_lives <= 0) {
            OnFinish(false);
        }
    }

    [Serializable]
    public struct Quiz {
        public Question this[int i] {
            get { return questions[i]; }
        }

        public int Length {
            get { return questions.Length; }
        }

        public int lives;
        public Question[] questions;
    }

    [Serializable]
    public struct Question {
        public string question;
        public string[] answers;
        public int correctId;
    }
}
