using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cohort.GameRunner.Minigames.Tester {
    public class Tester : Minigame {
        [SerializeField] private Slider _score;

        protected override float CorrectVisualDuration {
            get { return 0f; }
        }
        protected override float FaultiveVisualDuration {
            get { return 0f; }
        }
        protected override float FinishedVisualDuration {
            get { return 0f; }
        }

        public override int Score {
            get { return _scoreRange.GetValueRound(_score.value);}
            set { _score.value = _scoreRange.GetTime(value); }
        }

        public void Fail() {
            Score = MinScore;
            
            FinishMinigame();
        }
    }
}