using Cohort.Games.Planes;
using Cohort.Tools.Timers;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Monobehaviour class of the timer component.
/// </summary>
public class MonoTimer : MonoBehaviour
{
    [SerializeField] protected float _duration;
    [SerializeField] private bool _startOnAwake;
    
    public UnityEvent onStart;
    public UnityEvent onFinish;

    protected Timer _timer;

    private bool _hasCountdown;
    private Countdown _countdown;

    protected virtual void Awake() {
        if (_startOnAwake) {
            StartTimer();
        }
    }

    /// <summary>
    /// Starts the timer (restarts if already timing).
    /// </summary>
    public virtual void StartTimer() {
        if (_timer == null) {
            _timer = new Timer(_duration, true, FinishTimer);
        } else if (_timer.Active) {
            Debug.LogWarning($"Timer was already started ({gameObject.name}).");
            return;
        }
        
        _timer.duration = _duration;
        _timer.Start();
        Debug.LogError($"Started timer {_timer.duration} -> {_timer.Elapsed}");
        
        if (_hasCountdown) {
            _countdown.StartCountdown();
        }
        onStart?.Invoke();
    }

    /// <summary>
    /// Resets the timer, so it's ready for use again (keeps running if it was already running, doesn't if it was already stopped).
    /// </summary>
    public virtual void ResetTimer() {
        _timer?.Reset();
    }

    /// <summary>
    /// Stop the timer from running without event invoke.
    /// </summary>
    public virtual void StopTimer() {
        _timer?.Stop();
        Debug.LogError($"Stopped timer");
    }

    /// <summary>
    /// Stops the timer from running and invokes the event.
    /// </summary>
    public virtual void FinishTimer() {
        if (!_timer.HasFinished)
            _timer?.Stop();
        
        onFinish?.Invoke();
    }
}
