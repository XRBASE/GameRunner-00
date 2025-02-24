using UnityEngine;

namespace Cohort.GameRunner.Minigames {
    
    /// <summary>
    /// In scene handle class to access methods in the Minigame manager class.
    /// </summary>
    public class MinigameHandle : MonoBehaviour {
        public void StartMingameByIndex(int index) {
            MinigameManager.Instance.StartMinigameById(index);
        }
    }
}