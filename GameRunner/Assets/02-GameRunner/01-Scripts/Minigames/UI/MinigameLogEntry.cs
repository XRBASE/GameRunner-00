using System.Collections;
using Cohort.Patterns;
using Cohort.UI.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameLogEntry : MonoBehaviour, ObjectPool<string, MinigameLogEntry>.IPoolable {
	private const float DEATH_TIME = 1.5f;
	
	public bool IsActive {
		get { return gameObject.activeSelf;}
		set { gameObject.SetActive(value); }
	}

	[SerializeField] private TMP_Text _field;
	[SerializeField] private Toggle _tgl;

	public void UpdatePoolable(int index, string data) {
		_field.text = data;
		_tgl.isOn = false;
	}

	public void FinishTask(MiniGameDescription.State state) {
		switch (state) {
			case MiniGameDescription.State.Completed:
				_tgl.isOn = true;
				transform.SetAsFirstSibling();
				StartCoroutine(WaitAndRemove());
				return;
			case MiniGameDescription.State.Failed:
				RemoveTask();
				return;
		}
	}

	private void RemoveTask() {
		UILocator.Get<MinigameUILog>().RemoveEntry(this);
	}

	private IEnumerator WaitAndRemove() {
		yield return new WaitForSeconds(DEATH_TIME);

		RemoveTask();
	}

	public MinigameLogEntry Copy() {
		return Instantiate(this, transform.parent);
	}
}
