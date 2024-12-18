using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cohort.GameRunner.Minigames.Wordle {
    public class Word : MonoBehaviour {
        public Letter letterPrefab;
        private List<Letter> _letters = new List<Letter>();
        private char _empty = new char();
        private const float CORRECT_FEEDBACK_INTERVAL = .2f;
        private IEnumerator _feedbackRoutine;


        public void Initialise(float length, AudioSource feedBackAudioSource) {
            for (int i = 0; i < length; i++) {
                var letter = Instantiate(letterPrefab, transform);
                letter.SetAudioSource(feedBackAudioSource);
                _letters.Add(letter);
            }
        }

        private void SetLetter(int index, char letter) {
            _letters[index].SetLetterText(letter);
            _letters[index].SetLetterIndex(index);
        }

        public void AddLetter(int index, char letter) {
            SetLetter(index, letter);
            _letters[index].AddFeedback();
        }

        public void ClearLetter(int index) {
            SetLetter(index, _empty);
            _letters[index].RemoveFeedback();
        }

        public void ClearAllLetters() {
            for (int i = 0; i < _letters.Count; i++) {
                SetLetter(i, _empty);
            }

            _letters.First().RemoveFeedback();
        }

        public bool CheckWord(string word) {
            bool correct = true;
            foreach (var letter in _letters) {
                if (!letter.CheckLetter(word)) {
                    correct = false;
                }
            }

            return correct;
        }

        public void InCorrectFeedback() {
            foreach (var t in _letters) {
                t.IncorrectFeedback();
            }
        }


        public void DoCorrectFeedbackRoutine() {
            if (_feedbackRoutine != null)
                StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = CorrectFeedbackRoutine();
            StartCoroutine(_feedbackRoutine);
        }

        private IEnumerator CorrectFeedbackRoutine() {
            for (int i = _letters.Count - 1; i >= 0; i--) {
                _letters[i].CorrectFeedback();
                yield return new WaitForSeconds(CORRECT_FEEDBACK_INTERVAL);
            }
        }
    }
}