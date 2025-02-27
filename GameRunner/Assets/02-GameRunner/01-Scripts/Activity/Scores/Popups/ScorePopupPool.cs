using Cohort.Patterns;
using UnityEngine;

namespace Cohort.GameRunner.Score {
    public class ScorePopupPool : MonoBehaviour {
        [SerializeField] private ScorePopup _template;
        private ObjectPool<int, ScorePopup> _pool;

        private void Awake() {
            _pool = new ObjectPool<int, ScorePopup>(_template);
        }

        public void ShowPointsEarned(int points) {
            _pool.AddItem(points);
        }
    }
}