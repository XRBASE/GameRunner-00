using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "InteractiveVideoData", menuName = "Cohort/InteractiveVideoData")]
public class InteractiveVideoDataSO : ScriptableObject
{
    public InteractiveVideoData interactiveVideoData;

    public string InteractiveVideoDataJson()
    {
        return JsonUtility.ToJson(interactiveVideoData);
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(InteractiveVideoDataSO))]
    private class WordGameDataSoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var instance = (InteractiveVideoDataSO) target;

            if (GUILayout.Button("Copy Interactive Video Data to clipboard"))
            {
                GUIUtility.systemCopyBuffer = instance.InteractiveVideoDataJson();
            }
        }
    }
#endif
}
