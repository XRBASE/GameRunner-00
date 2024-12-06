using UnityEngine;

public class SceneConfiguration : MonoBehaviour {
    public LearningCycleDescription Learning {
        get { return _learning; }
    }
    
    [SerializeField] private LearningCycleDescription _learning;

    private void Awake() {
        LearningManager.Instance.Setting = Learning;
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
        
        _learning?.OnValidate();
    }

#endif
}
