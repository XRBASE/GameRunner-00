
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Base.Ravel.Utils
{
    
    public class CohortDashboard : EditorWindow
    {
        private SceneContainer _sceneContainer;

        public SceneContainer SceneContainer
        {
            get
            {
                if (_sceneContainer == null)
                    _sceneContainer = Resources.Load<SceneContainer>("Config/SO_SceneContainer");
                return _sceneContainer;
            }
            set => _sceneContainer = value;
        }
        

        [MenuItem("Cohort/Scene Dashboard #&d")]
        private static void Init()
        {
            EditorWindow window = (CohortDashboard) EditorWindow.GetWindow(typeof(CohortDashboard));
            window.titleContent = new GUIContent("Dashboard", "Use this dashboard to navigate scenes");
            window.Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Persistent Data"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
            if (!EditorApplication.isPlaying)
            {
                DrawScenes();
                DrawQuickStart();
            }
        }

      
        

        private void DrawQuickStart()
        {
            GUILayout.Label("Quick Start");
            if (GUILayout.Button("Play"))
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    return;
                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);
                EditorApplication.EnterPlaymode();

            }
        }

        private void DrawScenes()
        {
            GUILayout.Space(15);
            GUILayout.Label("Scene Switcher");
            foreach (var scene in SceneContainer.Scenes)
            {
                var item = AssetDatabase.GetAssetPath(scene);
                if (GUILayout.Button(
                    item.Substring(item.LastIndexOf('/') + 1).Replace(".unity", string.Empty)))
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(item);
                    }
                }
            }
        }
        
    }
}
#endif

