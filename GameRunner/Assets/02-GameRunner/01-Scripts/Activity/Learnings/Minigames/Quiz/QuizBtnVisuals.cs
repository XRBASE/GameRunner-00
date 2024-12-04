using Cohort.GameRunner.Input;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuizBtnVisuals : MonoBehaviour {
	[SerializeField] private float _duration;
	[SerializeField] private Color _correctCol, _incorrectCol;

	private Button _btn;
	private RectTransform _rt;
	private bool _inClick;

	private void Awake() {
		_rt = (RectTransform)transform;
		
		_btn = GetComponent<Button>();
		_btn.onClick.AddListener(LeftUp);
	}

	private void Start() {
		InputManager.Instance.Cursor.leftDown += LeftDown;
	}

	private void OnDestroy() {
		InputManager.Instance.Cursor.leftDown -= LeftDown;
	}

	public void ResetValues() {
		if (enabled) {
			Debug.LogError("Reset");
		}
	}

	public void HighlightBtn() {
		if (enabled) {
			Debug.LogError("Highlight");
		}
	}

	public void ClickBtn() {
		_inClick = true;
		if (enabled) {
			Debug.LogError("Click");
		}
	}

	public void ReleaseBtn() {
		if (enabled) {
			Debug.LogError("Release");
		}
	}

	
	
	private void LeftDown() {
		if (PointerOver()) {
			ClickBtn();
		}
		else {
			if (enabled) {
				Debug.LogWarning("Outside");
			}
		}
	}
	
	private void LeftUp() {
		if (_inClick) {
			ReleaseBtn();			
		}

		_inClick = false;
	}

	private bool PointerOver() {
		Vector2 localPos = _rt.InverseTransformPoint(InputManager.Instance.Cursor.ScreenPosition);
		return _rt.rect.Contains(localPos);
	}

	private enum State {
		None = 0,
		Hover,
		Hold,
		Release,
		Highlight
	}
}
