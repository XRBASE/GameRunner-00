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

        public override float Score {
            get { return _score.value;}
            set { _score.value = value; }
        }

        public void Fail() {
            _onFinished?.Invoke(0f);
        }
    }
}