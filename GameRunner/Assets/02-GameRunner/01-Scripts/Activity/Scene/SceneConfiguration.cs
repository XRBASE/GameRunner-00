using UnityEngine;
using Cohort.GameRunner.Minigames;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(101)] //Before ActivityLoader
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
    
    [CustomEditor(typeof(SceneConfiguration))]
    private class SceneConfigurationEditor : Editor {
        private SceneConfiguration _instance;

        private void OnEnable() {
            _instance = (SceneConfiguration)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Sort mingames")) {
                _instance.minigame?.minigames.Sort(MinigameDescription.SortPhase);
            }
        }
    }

#endif
}
