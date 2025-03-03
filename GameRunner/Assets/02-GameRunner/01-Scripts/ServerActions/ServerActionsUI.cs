using Cohort.UI.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(101)] //After gameloader
public class ServerActionsUI : UIPanel
{
	private const float ANIM_DURATION = 0.5f;

	private bool Open { get; set; }
	
	[SerializeField] private Button _toggleBtn;
	[SerializeField] private Button _startBtn;
	[SerializeField] private Button _stopBtn;
	private Vector2 _openAnchorPos, _closeAnchorPos;
	
	private void Awake() {
		UILocator.Register(this);
		
		_openAnchorPos = RectTransform.anchorMax;
		_closeAnchorPos = new Vector2(_openAnchorPos.x, 0f);
		
		_toggleBtn.onClick.AddListener(ToggleState);
		_startBtn.onClick.AddListener(StartGame);
		_stopBtn.onClick.AddListener(StopGame);
		
		DeactivateInstant();
	}

	private void OnDestroy() {
		UILocator.Remove<ServerActionsUI>();
		
		_toggleBtn.onClick.RemoveListener(ToggleState);
		_startBtn.onClick.RemoveListener(StartGame);
		_stopBtn.onClick.RemoveListener(StopGame);
	}
	
	private void StopGame() {
		ActivityLoader.Instance.StopActivity();
	}

	private void StartGame() { ;
		ActivityLoader.Instance.LoadActivity();
	}

	public void ToggleState() {
		if (Open)
			Deactivate();
		else 
			Activate();
	}

	public override void Activate() {
		RectTransform.DOAnchorMax(_openAnchorPos, ANIM_DURATION);
		Open = true;
	}

	private void DeactivateInstant() {
		RectTransform.DOAnchorMax(_closeAnchorPos, 0);
	}

	public override void Deactivate() {
		RectTransform.DOAnchorMax(_closeAnchorPos, ANIM_DURATION);
		Open = false;
	}
}
