using Cohort.Ravel.Patterns;
using UnityEngine;
using UnityEngine.UI;

public class ShiftingSample : Singleton<ShiftingSample> {
	[SerializeField] private Image _on, _off;

	public void Shift(bool isShifting) {
		_on.gameObject.SetActive(isShifting);
		_off.gameObject.SetActive(!isShifting);
	}
}
