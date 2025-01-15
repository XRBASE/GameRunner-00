using UnityEngine;

using Cohort.GameRunner.Minigames;

public class SceneConfiguration : MonoBehaviour {
    public MinigameCycleDescription Minigame {
        get { return minigame; }
    }
    
    [SerializeField] private MinigameCycleDescription minigame;

    private void Awake() {
        MinigameManager.Instance.Setting = Minigame;
    }
    
#if UNITY_EDITOR
    public void OnValidate() {
        SceneConfiguration[] configs = FindObjectsOfType<SceneConfiguration>(); 
        if (configs.Length > 1) {
            string names = "";
            for (int i = 0; i < configs.Length; i++) {
                names += $"{configs[i].gameObject.name},";
            }

            names = names.Substring(0, names.Length - 1) + ".";
            Debug.LogError($"No two scene configurations can exist at the same time!\n configs found on {names}");
        }
        
        minigame?.OnValidate();
    }

#endif
}
