using System;
using Cohort.GameRunner.Minigames;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Provides simple interface to filter 
/// </summary>
public class MinigameFinishedCheck : MonoBehaviour {
    
    [SerializeField, Tooltip("Enables object when game was finished for any cause that is added to the filter.")] 
    private Minigame.FinishCause causeFilter;
    [SerializeField, Tooltip("Enables object when score is above 'x'. Negative value negates the filter.")] 
    private float _minScore = -1;
    [SerializeField, Tooltip("Enables object when score is below 'x'. Negative value negates the filter.")]
    private float _maxScore = -1;

    public UnityEvent onCheckPassed;
    public UnityEvent onCheckFailed;

    private void Start() {
        MinigameManager.Instance.onMinigameFinished += CheckValue;
    }

    private void OnDestroy() {
        MinigameManager.Instance.onMinigameFinished -= CheckValue;
    }

    public void CheckValue(Minigame.FinishCause cause, float score) {
        bool pass = (cause & causeFilter) > 0;
        
        if (pass && _minScore >= 0) {
            pass = score >= _minScore;
        }
            
        if (pass && _maxScore >= 0) {
            pass = score <= _maxScore;
        }

        if (pass) {
            onCheckPassed?.Invoke();
        }
        else {
            onCheckFailed?.Invoke();
        }
    }
}
