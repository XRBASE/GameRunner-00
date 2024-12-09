using System.Collections;
using Cohort.CustomAttributes;
using Cohort.GameRunner.Input;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuizBtnVisuals : MonoBehaviour {
	[SerializeField] private float _duration = 1f;
	[SerializeField] private Graphic[] _graphics;
	
	[SerializeField] private Color _correctCol;
	[SerializeField] private Color _incorrectCol;
	[SerializeField] private Color _hoverCol; 
	[SerializeField] private Color _holdCol;
	
	private Color _default;
	private Color _highlightCol;
	
	private Button _btn;
	private RectTransform _rt;
	
	private bool _inClick;
	
	[SerializeField, ReadOnly] private State _state;
	private StateData _startData;
	private float _timer;

	private Camera _cam;
	
	private void Awake() {
		_rt = (RectTransform)transform;
		
		_btn = GetComponent<Button>();
		_btn.onClick.AddListener(LeftUp);

		_default = _graphics[0].color;
		_state = State.None;
		_startData = GetData(_state);
		
		_cam = Camera.main;
	}

	private void Start() {
		InputManager.Instance.Cursor.leftDown += LeftDown;
	}

	private void OnDestroy() {
		if (!InputManager.Disposed)
			InputManager.Instance.Cursor.leftDown -= LeftDown;
	}

	private void Update() {
		if (PointerOver()) {
			if (_state == State.None)
				SetState(State.Hover);
		}
		else {
			if (_state == State.Hover || _state == State.Hold) {
				SetState(State.None);
			}
		}
		
		if (_timer > _duration)
			return;

		_timer += Time.deltaTime;
		ApplyVisuals(Mathf.Clamp01(_timer / _duration));
	}

	private void ApplyVisuals(float t) {
		StateData data = GetData(_state);
		
		Color c = Color.Lerp(_startData.col, data.col, t);
		for (int i = 0; i < _graphics.Length; i++) {
			_graphics[i].color = c;
			_graphics[i].transform.localScale = Vector3.Lerp(_startData.scale, data.scale, t);
		}
	}

	private void SetState(State s) {
		_state = s;
		
		_startData = new StateData();
		_startData.col = _graphics[0].color;
		_startData.scale = _btn.transform.localScale;
		
		_timer = 0f;
	}
	
	public void ResetValues() {
		_state = State.None;
		ApplyVisuals(1f);
		
		_timer = 0f;
	}

	public void HighlightBtn(bool correct) {
		_highlightCol = (correct) ? _correctCol : _incorrectCol;

		//gives some extra time for the hold visual to show up!
		StartCoroutine(WaitForHighlight());
	}

	private IEnumerator WaitForHighlight() {
		yield return new WaitForSeconds(_duration * 0.5f);
		
		SetState(State.Highlight);
	}

	public void HoldBtn() {
		if (_state == State.Highlight)
			return;
			
		_inClick = true;
		
		SetState(State.Hold);
	}

	public void ReleaseBtn() {
		if (_state == State.Highlight)
			return;
		
		if (_state == State.Hold) {
			SetState(State.None);
		}
	}
	
	private void LeftDown() {
		if (PointerOver()) {
			HoldBtn();
		}
	}
	
	private void LeftUp() {
		if (_inClick) {
			ReleaseBtn();			
		}

		_inClick = false;
	}

	private bool PointerOver() {
		Vector2 localPos;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, InputManager.Instance.Cursor.ScreenPosition,
		                                                        _cam, out localPos);
		return _rt.rect.Contains(localPos);
	}
	
	private StateData GetData(State state) {
		StateData data = new StateData();
		data.scale = Vector3.one;
		switch (state) {
			case State.None: 
			default:
				data.col = _default;
				break;
			case State.Hover:
				data.col = _hoverCol;
				break;
			case State.Hold:
				data.col = _holdCol;
				data.scale = new Vector3(0.8f, 0.8f, 0.8f);
				break;
			case State.Highlight:
				data.col = _highlightCol;
				break;
		}

		return data;
	}
	
	private enum State {
		None = 0,
		Hover,
		Hold,
		Highlight
	}

	private struct StateData {
		public Color col;
		public Vector3 scale;
	}
}
