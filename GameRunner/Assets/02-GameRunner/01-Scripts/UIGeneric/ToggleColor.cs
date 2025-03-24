using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleColor : MonoBehaviour
{
	public bool IsOn {
		get { return _isOn; }
		set {
			if (_isOn == value)
				return;
            
			_isOn = value;
			_graphic.color = (_isOn) ? _onCol : _offCol;
            
			onValueChanged?.Invoke(_isOn);
		}
	}
    
	[SerializeField] private bool _isOn;
	[SerializeField] private Graphic _graphic;
	[SerializeField] private Color _onCol;
	[SerializeField] private Color _offCol;
	[SerializeField] private UnityEvent<bool> onValueChanged;

	private void Start()
	{
		//set correct start sprite
		_graphic.color = (_isOn) ? _onCol : _offCol;
	}

	public void ToggleValue() {
		IsOn = !_isOn;
	}
}
