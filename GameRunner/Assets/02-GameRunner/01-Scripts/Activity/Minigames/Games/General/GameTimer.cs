using System;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {
    private const string FORMAT = @"mm\:ss\:ff";
    private const string PREFIX = "time left\t";
    
    [SerializeField] private TMP_Text _timeLeftField;

    private DateTime _goal;
    private TimeSpan _remaining;
    private Action _onComplete;

    private void Awake() {
        enabled = false;
    }

    private void OnDestroy() {
        _onComplete = null;
    }

    public void Initialize(float duration, Action onComplete) {
        int sec = (int)duration;
        int milisec = (int)((duration - sec) * 1000);
        
        _goal = DateTime.Now + new TimeSpan(0, 0, 0, sec, milisec);
        _onComplete = onComplete;

        enabled = true;
    }

    private void Update() {
        if (DateTime.Now < _goal) {
            _remaining = _goal - DateTime.Now;
        }
        else {
            _remaining = new TimeSpan();
            
            _onComplete?.Invoke();
            enabled = false;
        }
        
        _timeLeftField.text = PREFIX + _remaining.ToString(FORMAT);
    }
}
