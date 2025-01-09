using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cohort.GameRunner.Minigames.Tester {
    public class Tester : Minigame {
        [SerializeField] private Slider _score;

        public override float Score {
            get { return _score.value;}
            set { _score.value = value; }
        }

        public void Fail() {
            _onFinished?.Invoke(0f);
        }
    }
}