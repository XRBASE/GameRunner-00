using System;
using Cohort.GameRunner.Input;
using Cohort.Tools.Timers;
using UnityEngine;

public class MouseOverEvent : MonoBehaviour
{
    public bool IsMouseOver { get; private set; }
    
    public Action onMouseEnter;
    public Action onMouseExit;

    [SerializeField] private float _delayDuration = 2f;
    private Timer _delayTimer;
    private bool _hasDelay;
    private bool _inDelay;
    
    private Rect _rect;

    private void Awake()
    {
        _hasDelay = _delayDuration > 0f;
        if (_hasDelay) {
            _delayTimer = new Timer(_delayDuration, false, OnExit);
        }
    }

    private void Update()
    {
        bool mouseOver = RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform,
                                                                           InputManager.Instance.LearningCursor.ScreenPosition);
        if (IsMouseOver != mouseOver) {
            if (mouseOver) {
                OnEnter();    
            }
            else {
                if (_hasDelay) {
                    if (!_inDelay) {
                        _inDelay = true;
                        _delayTimer.Reset();
                        _delayTimer.Start();
                    }
                }
                else {
                    OnExit();    
                }
            }
        }
        else {
            if (IsMouseOver && _inDelay) {
                _delayTimer.Stop();
                _inDelay = false;
            }
        }
    }

    private void OnEnter()
    {
        IsMouseOver = true;
        onMouseEnter?.Invoke();
    }

    private void OnExit()
    {
        IsMouseOver = false;
        _inDelay = false;
        onMouseExit?.Invoke();
    }
}
