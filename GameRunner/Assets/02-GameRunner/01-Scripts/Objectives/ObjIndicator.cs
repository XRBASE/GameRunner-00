using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(10)] //Before interactable
public class ObjIndicator : MonoBehaviour {
    [SerializeField] private Transform _target;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Color _color;
    [SerializeField] private Vector2 _size = new Vector2(75,75);
    [SerializeField] private bool _enabled = false;
    
    private Image _rnd;
    private RectTransform _rt;
    private RectTransform _parentRt;
    private Camera _cam;
    
    private Vector2 position;
    private Vector2 hSize;
    private Vector2 hMargin;

    private bool _initialized;

    private void Start() {
        SetUp(_enabled, _icon, _color, _size, _target);
    }

    private void OnDestroy() {
        //unregister
        if (_rt != null) {
            Destroy(_rt.gameObject);
        }
    }

    public void SetUp(bool enabled, Sprite icon, Color icoColor, Vector2 size, Transform target = null) {
        if (_initialized)
            return;
        
        if (target != null) {
            _target = target;
        }
        
        _rnd = UILocator.Get<ObjIndicatorPanel>().CreateIndicator(_size, _color, icon, gameObject.name); 
        
        _rt = (RectTransform)_rnd.transform;
        _parentRt = (RectTransform)_rt.parent;
        _cam = Camera.main;
        
        _initialized = true;
        SetActive(enabled, true);
    }

    public void SetActive(bool enabled, bool force = false) {
        if (!_initialized) {
            SetUp(enabled, _icon, _color, _size, _target);
            return;                              
        }
        
        if (force || _enabled != enabled) {
            if (enabled) {
                UILocator.Get<ObjIndicatorPanel>().OnActivate();
            }
            else {
                UILocator.Get<ObjIndicatorPanel>().OnDeactivate();
            }
        }
        _enabled = enabled;
        _rt.gameObject.SetActive(enabled);
    }

    private void Update() {
        if (!_enabled || _target == null) {
            _rnd.enabled = false;
            
            return;
        }
        
        hSize = (_parentRt.rect.size + -_parentRt.sizeDelta)  / 2f;
        hMargin = _parentRt.rect.size  / 2f;
        
        _rnd.enabled = true;
        Vector3 delta = _cam.transform.InverseTransformPoint(_target.position);
        
        position  = _cam.WorldToViewportPoint(_target.position);
        if (delta.z <= 0f) {
            position.y = 0f;
        }

        position = (position * 2) - new Vector2(1f, 1f);
        position *= hSize;

        position.x = Mathf.Clamp(position.x, -hMargin.x, hMargin.x);
        position.y = Mathf.Clamp(position.y, -hMargin.y, hMargin.y);

        _rt.anchoredPosition = position;
    }
}
