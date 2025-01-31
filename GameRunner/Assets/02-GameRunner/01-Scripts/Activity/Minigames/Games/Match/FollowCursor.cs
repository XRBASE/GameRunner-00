using System;
using Cohort.GameRunner.Input;
using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    private RectTransform _rectTransform;
    private Vector2 _position;
    public Canvas canvas;
    public Camera camera;
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        camera = Camera.main;
    }

    private void Update()
    {
        _position.Set(InputManager.Instance.LearningCursor.ScreenPosition.x,InputManager.Instance.LearningCursor.ScreenPosition.y);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, 
            _position, 
            camera, 
            out localPoint);

        _rectTransform.anchoredPosition = localPoint;
    }
}
