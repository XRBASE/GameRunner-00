using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace Cohort.GameRunner.Minigames.Wordle {
    public class WordGame : Minigame {
        // Enum to define game modes: Random or Sequential word selection
        public enum WordGameMode {
            Random,
            Sequential
        };

        // Constants for feedback timeout durations
        private const float INVALID_FEEDBACK_TIMEOUT = 1f;
        private const float GAME_COMPLETE_FEEDBACK_TIMEOUT = 2f;

        // Property to get the current word to be played
        private Word CurrentWord => _words[_entryIndex];

        // Property to check if the game can continue
        private bool CanPlay => _isPlaying && _entryIndex < _words.Count;

        // Public variables for game objects and UI components
        public Word wordPrefab;
        public WordGameInput wordGameInput;
        public Transform gameParent;
        public TextMeshProUGUI title;
        public AudioSource feedbackAudio;
        public AudioClip successAudioClip, failureAudioClip;
        public ScoreUI scoreUI;
        public TextMeshProUGUI hintText;
        public PlayableDirector invalidWordFeedback;

        // Private variables to store game data
        private readonly List<Word> _words = new List<Word>();
        private List<string> _wordDictionary;
        private WordGameData _wordGameData;
        private Action<float> _onGameFinished;
        private Action<float> _testOnGameFinished;
        private WordData _chosenWord;
        private IEnumerator _feedBackTimeoutRoutine;
        private string _text = string.Empty;
        private int _entryIndex;
        private float _completionPercent;
        private float _scoreMultiplier;
        private bool _isPlaying;
        private int _currentPuzzle;
        private bool _pickRandomWord;
        private int _attempts;
        private WordGameMode _wordGameMode;

        // Method to initialize the game with data, score multiplier, and callback on game finish
        public override void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished) {
            _wordGameData = JsonUtility.FromJson<WordGameData>(gameData);
            if (_wordGameData == null || _wordGameData.wordList == null || _wordGameData.wordList.Count == 0) {
                Debug.LogError("Invalid game data!");
                return;
            }

            _wordGameMode = _wordGameData.wordGameMode;
            _onGameFinished = onGameFinished;
            _scoreMultiplier = scoreMultiplier;
            title.text = _wordGameData.title;
            BuildGame();
        }

        // Method that runs when the script is first initialized
        protected override void Awake() {
            base.Awake();
            wordGameInput.onKeyboardInput += ValueChanged;
            wordGameInput.remove += RemoveLastLetter;
        }

        protected void OnDestroy() {
            wordGameInput.onKeyboardInput -= ValueChanged;
            wordGameInput.remove -= RemoveLastLetter;
        }

        // Proceed to the next puzzle or complete the game if there are no more puzzles
        private void NextPuzzle() {
            _currentPuzzle++;
            if (_currentPuzzle < _wordGameData.puzzleAmount && _currentPuzzle < _wordGameData.wordList.Count) {
                DoFeedbackTimeout(GAME_COMPLETE_FEEDBACK_TIMEOUT, ResetGame);
            }
            else {
                HandleGameComplete();
            }
        }

        // Reset the game by clearing the current state and building it again
        private void ResetGame() {
            ClearGame();
            BuildGame();
        }

        // Display the score after each puzzle or at the end
        private void DisplayScore(float score) {
            scoreUI.PlayScore((int)(score * _scoreMultiplier));
        }

        // Build the game by selecting a word and creating necessary UI components
        private void BuildGame() {
            _chosenWord = GetWord();
            _chosenWord.word = _chosenWord.word.ToLower();
            _wordDictionary = WordDictionary.GetWordDictionary(_chosenWord.word.Length);

            // Create multiple word objects for each try and initialize them
            for (int i = 0; i < _wordGameData.tries; i++) {
                var word = Instantiate(wordPrefab, gameParent);
                _words.Add(word);
                word.Initialise(_chosenWord.word.Length, feedbackAudio);
            }

            hintText.text = _chosenWord.hint;
            _entryIndex = 0;
            _isPlaying = true;
        }

        // Clear all game elements (words and UI) when resetting or finishing the game
        private void ClearGame() {
            if (_words.Count > 0) {
                foreach (var word in _words) {
                    Destroy(word.gameObject);
                }

                _words.Clear();
            }
        }

        // Choose a word based on the game mode: Random or Sequential
        private WordData GetWord() {
            switch (_wordGameMode) {
                case WordGameMode.Random:
                    return _wordGameData.wordList[Random.Range(0, _wordGameData.wordList.Count)];
                case WordGameMode.Sequential:
                    return _wordGameData.wordList[_currentPuzzle];
                default:
                    return _wordGameData.wordList[Random.Range(0, _wordGameData.wordList.Count)];
            }
        }

        // Submit the current word input and check if it's valid or not
        private void Submit() {
            if (!CanPlay || _text.Length < _chosenWord.word.Length)
                return;

            if (!_wordDictionary.Contains(_text)) {
                HandleAnswerInvalid();
                return;
            }

            _attempts++;
            HandleAnswer(CurrentWord.CheckWord(_chosenWord.word));

        }

        // Handle invalid word input and give feedback
        private void HandleAnswerInvalid() {
            CurrentWord.InCorrectFeedback();
            feedbackAudio.PlayOneShot(failureAudioClip);
            PlayableDirectorFeedback(invalidWordFeedback);
            wordGameInput.SetInputActive(false);
            DoFeedbackTimeout(INVALID_FEEDBACK_TIMEOUT, FeedbackTimeout);
        }

        // Play an animation feedback using a PlayableDirector
        private void PlayableDirectorFeedback(PlayableDirector playableDirector) {
            playableDirector.time = 0;
            playableDirector.Stop();
            playableDirector.Evaluate();
            playableDirector.Play();
        }

        // Timeout handler to reset input and clear the word after invalid feedback
        private void FeedbackTimeout() {
            CurrentWord.ClearAllLetters();
            _text = string.Empty;
            wordGameInput.SetInputActive(true);
        }

        // Handle the result of the word submission (correct or incorrect)
        private void HandleAnswer(bool correct) {
            if (correct) {
                CurrentWord.DoCorrectFeedbackRoutine(); // Show correct feedback
                feedbackAudio.PlayOneShot(successAudioClip); // Play success audio
                NextPuzzle(); // Move to the next puzzle
            }
            else {
                CurrentWord.InCorrectFeedback(); // Show incorrect feedback
                feedbackAudio.PlayOneShot(failureAudioClip); // Play failure audio
            }

            _text = string.Empty;
            _entryIndex += 1;
            if (_entryIndex >= _words.Count) {
                NextPuzzle();
            }
        }

        // Handle the completion of the game (show score and finish)
        private void HandleGameComplete() {
            _completionPercent = (_wordGameData.tries * _wordGameData.puzzleAmount - _attempts) /
                                 ((float)_wordGameData.tries * _wordGameData.puzzleAmount -
                                  _wordGameData.puzzleAmount); // Calculate completion percentage
            DisplayScore(_completionPercent);
            _isPlaying = false;
            DoFeedbackTimeout(GAME_COMPLETE_FEEDBACK_TIMEOUT, GameFinished);
        }

        // Final callback after the game is finished
        private void GameFinished() {
            _onGameFinished?.Invoke(_completionPercent);
        }

        public override void StopMinigame() {
            _onGameFinished?.Invoke(_completionPercent);
        }

        // Handle the input of a single character from the keyboard
        private void ValueChanged(string text) {
            if (!CanPlay || (_text + text).Length > _chosenWord.word.Length)
                return;

            _text += text;
            CurrentWord.AddLetter(_text.Length - 1, _text.Last());

            if (_text.Length == _chosenWord.word.Length) {
                Submit();
            }
        }

        // Remove the last character from the input text
        private void RemoveLastLetter() {
            if (_text.Length - 1 < 0)
                return;

            CurrentWord.ClearLetter(_text.Length - 1);
            _text = _text.Substring(0, _text.Length - 1);
        }

        private void DoFeedbackTimeout(float time, Action onFeedbackTimeout) {
            if (_feedBackTimeoutRoutine != null) {
                StopCoroutine(_feedBackTimeoutRoutine);
            }

            _feedBackTimeoutRoutine = FeedbackTimeOutRoutine(time, onFeedbackTimeout);
            StartCoroutine(_feedBackTimeoutRoutine);
        }


        // Coroutine to handle the timeout period
        private IEnumerator FeedbackTimeOutRoutine(float time, Action onFeedbackTimeout) {
            yield return new WaitForSeconds(time);
            onFeedbackTimeout?.Invoke();
        }
    }
}