using UnityEngine;

public class SlidingPanel : MonoBehaviour {
    private const float LERP_SPEED = 0.01f;
    
    [SerializeField] private MouseOverEvent _activator;
    [SerializeField] private RectTransform _target;
    
    [SerializeField] private Vector2 closedPos;
    [SerializeField] private Vector2 openPos;
    private Vector2 _targetPos;

    [SerializeField] private bool isOpen;
    private float _curT;
    private bool fin;

    private void Awake() {
        _activator.onMouseEnter += OnMouseEnter;
        _activator.onMouseExit += OnMouseExit;

        _targetPos = isOpen ? openPos : closedPos;
        fin = (_target.anchoredPosition - _targetPos).magnitude < 0.1f;
    }

    private void OnDestroy() {
        _activator.onMouseEnter -= OnMouseEnter;
        _activator.onMouseExit -= OnMouseExit;
    }

    private void Update() {
        if (fin)
            return;

        _targetPos = isOpen ? openPos : closedPos;
        
        _curT += LERP_SPEED;
        if (_curT >= 1f) {
            _curT = 1f;
            fin = true;
        }
        
        _target.anchoredPosition = Vector2.Lerp(_target.anchoredPosition, _targetPos, _curT);
    }
    
    private void OnMouseEnter() {
        isOpen = true;
        
        _curT = Mathf.Clamp01(1f - _curT);
        fin = false;
    }
    
    private void OnMouseExit() {
        isOpen = false;
        
        _curT = Mathf.Clamp01(1f - _curT);
        fin = false;
    }
}
