using System;
using UnityEngine;
using UnityEngine.UI;

public class Tester : Learning {
    [SerializeField] private Slider _score;
    
    private Action<float> _onLearningFinished;
    
    public override void Initialize(string gameData, int scoreMultiplier, Action<float> onLearningFinished) {
        _onLearningFinished += onLearningFinished;
    }

    public void Complete() {
        _onLearningFinished?.Invoke(_score.value);
    }

    public void Fail() {
        _onLearningFinished?.Invoke(0f);
    }
}
