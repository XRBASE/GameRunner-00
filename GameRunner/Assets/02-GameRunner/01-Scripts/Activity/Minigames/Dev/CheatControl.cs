using UnityEngine;

namespace Cohort.GameRunner.Minigames {
    public class CheatButton : MonoBehaviour {
        [SerializeField] private Minigame _controller;

        public void WinGame() {
            _controller.Score = _controller.MaxScore;
            _controller.FinishMinigame();
        }

        public void LoseGame() {
            _controller.Score = _controller.MinScore;
            _controller.FinishMinigame();
        }
    }
}