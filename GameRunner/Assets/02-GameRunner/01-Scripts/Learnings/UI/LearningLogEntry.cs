using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearningLogEntry : MonoBehaviour {
    private const float DEATH_TIME = 1.5f;

    public string Text { get { return _field.text; } set { _field.text = value; } }

    [SerializeField] private TMP_Text _field;
    [SerializeField] private Toggle _tgl;

    public void CheckLogItem(LearningDescription.State state) {
        switch (state) {
            case LearningDescription.State.Completed:
                StartCoroutine(WaitAndRemove());
                break;
            case LearningDescription.State.Failed:
                RemoveTask();
                break;
            default:
                Debug.LogError($"No check implemented for learning type: {state}");
                break;
        }
    }

    private void RemoveTask() {
        Destroy(gameObject);
    }

    private IEnumerator WaitAndRemove() {
        _tgl.isOn = true;
        
        yield return new WaitForSeconds(DEATH_TIME);
        RemoveTask();
    }
}
