using Base.Ravel.Games.Planes;
using Cohort.Tools.Timers;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Monobehaviour class of the timer component.
/// </summary>
public class MonoTimer : MonoBehaviour
{
    [SerializeField] private float _duration;
    [SerializeField] private bool _startOnAwake;
    [SerializeField] private UnityEvent _onStart;
    [SerializeField] private UnityEvent _onFinish;

    private Timer _timer;

    private bool _hasCountdown;
    private Countdown _countdown;

    private void Awake() {
        if (_startOnAwake) {
            StartTimer();
        }
    }

    /// <summary>
    /// Starts the timer (restarts if already timing).
    /// </summary>
    public void StartTimer() {
        if (_timer == null) {
            _timer = new Timer(_duration, true, FinishTimer);
        }
        else {
            _timer.Reset();
            _timer.Start();
        }

        if (_hasCountdown) {
            _countdown.StartCountdown();
        }
        
        _onStart?.Invoke();
    }

    /// <summary>
    /// Resets the timer, so it's ready for use again (keeps running if it was already running, doesn't if it was already stopped).
    /// </summary>
    public void ResetTimer() {
        _timer?.Reset();
    }

    /// <summary>
    /// Stop the timer from running without event invoke.
    /// </summary>
    public void StopTimer() {
        _timer?.Stop();
    }

    /// <summary>
    /// Stops the timer from running and invokes the event.
    /// </summary>
    public void FinishTimer() {
        if (!_timer.HasFinished)
            _timer?.Stop();
        
        _onFinish?.Invoke();
    }
}
