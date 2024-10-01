using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleSprite : MonoBehaviour
{
    public bool IsOn {
        get { return _isOn; }
        set {
            if (_isOn == value)
                return;
            
            _isOn = value;
            _graphic.sprite = (_isOn) ? _onSpr : _offSpr;
            
            onValueChanged?.Invoke(_isOn);
        }
    }
    
    [SerializeField] private bool _isOn;
    [SerializeField] private Image _graphic;
    [SerializeField] private Sprite _onSpr;
    [SerializeField] private Sprite _offSpr;
    [SerializeField] private UnityEvent<bool> onValueChanged;

    private void Start()
    {
        //set correct start sprite
        _graphic.sprite = (_isOn) ? _onSpr : _offSpr;
    }

    public void ToggleValue() {
        IsOn = !_isOn;
    }
}
