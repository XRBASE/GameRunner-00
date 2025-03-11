using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    private RectTransform _rectTransform;
    public Canvas canvas;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out Vector2 pos);
        _rectTransform.anchoredPosition = pos;
    }
}
