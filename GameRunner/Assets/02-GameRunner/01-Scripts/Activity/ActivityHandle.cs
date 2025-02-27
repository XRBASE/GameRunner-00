using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// In scene access point for the activity loader.
/// </summary>
public class ActivityHandle : MonoBehaviour {
    public UnityEvent onActivityStart;
    public UnityEvent onActivityStop;

    private void Start() {
        ActivityLoader.Instance.onActivityStart += OnActivityStart;
        ActivityLoader.Instance.onActivityStop += OnActivityStop;

        if (ActivityLoader.Instance.InActivity) {
            OnActivityStart();
        }
        else {
            OnActivityStop();
        }
    }

    private void OnDestroy() {
        ActivityLoader.Instance.onActivityStart -= OnActivityStart;
        ActivityLoader.Instance.onActivityStop -= OnActivityStop;
    }

    private void OnActivityStart() {
        onActivityStart?.Invoke();
    }
    
    private void OnActivityStop() {
        onActivityStop?.Invoke();
    }

    public void StartActivity() {
        ActivityLoader.Instance.LoadActivity();
    }
    
    public void StopActivity() {
        ActivityLoader.Instance.StopActivity();
    }
}
