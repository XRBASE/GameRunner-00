using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Cohort.Patterns;

namespace Cohort.GameRunner.Minigames {
    public class MingameLogEntry : MonoBehaviour, ObjectPool<string, MingameLogEntry>.IPoolable {
        private const float DEATH_TIME = 1.5f;

        public bool IsActive {
            get { return gameObject.activeSelf; }
            set { gameObject.SetActive(value); }
        }

        private string Text {
            get { return _field.text; }
            set { _field.text = value; }
        }

        [SerializeField] private TMP_Text _field;
        [SerializeField] private Toggle _tgl;

        private ObjectPool<string, MingameLogEntry> _pool;

        public void Initialize(ObjectPool<string, MingameLogEntry> pool) {
            _pool = pool;
        }

        public void UpdatePoolable(int index, string data) {
            Text = data;
            _tgl.isOn = false;
        }

        public void CheckLogItem(MinigameDescription.Status state) {
            switch (state) {
                case MinigameDescription.Status.FinSuccess:
                    StartCoroutine(WaitAndRemove());
                    break;
                case MinigameDescription.Status.FinFailed:
                    RemoveDirect();
                    break;
                default:
                    Debug.LogError($"No check implemented for learning type: {state}");
                    break;
            }
        }

        public void RemoveDirect() {
            _pool.RemoveItem(this);
        }

        private IEnumerator WaitAndRemove() {
            _tgl.isOn = true;

            yield return new WaitForSeconds(DEATH_TIME);
            RemoveDirect();
        }

        public MingameLogEntry Copy() {
            return Instantiate(this, transform.parent);
        }
    }
}