using System.Collections;
using Cohort.Patterns;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LearningLogEntry : MonoBehaviour, ObjectPool<string,LearningLogEntry>.IPoolable {
    private const float DEATH_TIME = 1.5f;
    public bool IsActive {
        get { return gameObject.activeSelf;}
        set { gameObject.SetActive(value); }
    }
    
    private string Text { get { return _field.text; } set { _field.text = value; } }

    [SerializeField] private TMP_Text _field;
    [SerializeField] private Toggle _tgl;

    private ObjectPool<string, LearningLogEntry> _pool;

    public void Initialize(ObjectPool<string, LearningLogEntry> pool) {
        _pool = pool;
    }

    public void UpdatePoolable(int index, string data) {
        Text = data;
        _tgl.isOn = false;
    }
    
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
        _pool.RemoveItem(this);
    }

    private IEnumerator WaitAndRemove() {
        _tgl.isOn = true;
        
        yield return new WaitForSeconds(DEATH_TIME);
        RemoveTask();
    }

    public LearningLogEntry Copy() {
        return Instantiate(this, transform.parent);
    }
}
