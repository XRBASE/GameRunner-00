using UnityEngine;

namespace Cohort.GameRunner.Minigames {
    public class CheatButton : MonoBehaviour {
        [SerializeField] private Minigame _controller;

        public void WinGame() {
            _controller.Score = 1f;
            _controller.FinishMinigame();
        }

        public void LoseGame() {
            _controller.Score = 0f;
            _controller.FinishMinigame();
        }
    }
}