using Cohort.GameRunner.Input;
using UnityEngine;

public abstract class UIPointerOver : MonoBehaviour
{
    public bool PointerOver()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform) transform, InputManager.Instance.LearningCursor.ScreenPosition,
            Camera.main, out var localPos);
        return ((RectTransform) transform).rect.Contains(localPos);
    }
}