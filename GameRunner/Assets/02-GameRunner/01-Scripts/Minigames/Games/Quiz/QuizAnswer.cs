using System;
using Cohort.Patterns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuizAnswer : MonoBehaviour, ObjectPool<string, QuizAnswer>.IPoolable {
	public bool IsActive {
		get { return gameObject.activeSelf;}
		set { gameObject.SetActive(value); }
	}

	public bool Value {
		get { return _activator.isOn; }
		set { _activator.isOn = false; }
	}

	public Action<int, bool> onValueChanged;
	
	[SerializeField] private TMP_Text _field;
	[SerializeField] private Toggle _activator;

	private int _index;

	private void Awake() {
		_activator.onValueChanged.AddListener(OnValueChanged);
	}

	private void OnDestroy() {
		_activator.onValueChanged.RemoveListener(OnValueChanged);
	}

	public void UpdatePoolable(int index, string data) {
		_index = index;
		_field.text = data;
	}

	private void OnValueChanged(bool value) {
		onValueChanged?.Invoke(_index, value);
	}

	public QuizAnswer Copy() {
		QuizAnswer copy = Instantiate(this, transform.parent);
		copy.onValueChanged = onValueChanged;
		
		return copy;
	}
}
