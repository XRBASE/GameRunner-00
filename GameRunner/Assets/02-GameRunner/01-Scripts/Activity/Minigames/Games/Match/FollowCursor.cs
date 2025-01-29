using System;
using Cohort.GameRunner.Input;
using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    private RectTransform _rectTransform;
    private RectTransform _offsetTransform;
    private Vector3 _position;
    private bool _initialised;

    public void Initialise(RectTransform offsetTransform)
    {
        _rectTransform = GetComponent<RectTransform>();
        _offsetTransform = offsetTransform;
        _initialised = true;
    }

    private void Update()
    {
        if (!_initialised)
            return;
        _position.Set(InputManager.Instance.LearningCursor.ScreenPosition.x + _offsetTransform.position.x,InputManager.Instance.LearningCursor.ScreenPosition.y + _offsetTransform.position.y,0f);
        _rectTransform.anchoredPosition = _position;
    }
}
