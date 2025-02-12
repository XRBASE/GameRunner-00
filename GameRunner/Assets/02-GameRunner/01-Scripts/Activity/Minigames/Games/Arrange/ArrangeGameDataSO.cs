using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
[CreateAssetMenu(fileName = "ArrangeGameDataSO", menuName = "Cohort/ArrangeGameData")]
public class ArrangeGameDataSO : ScriptableObject
{
    public ArrangeGameData arrangeGameData;

    private string MatchGameDataJson()
    {
        arrangeGameData.chosenIds = arrangeGameData.ArrangeData.Select(pair => pair.UID).ToList();
        return JsonUtility.ToJson(arrangeGameData);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ArrangeGameDataSO))]
    private class WordGameDataSoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var instance = (ArrangeGameDataSO) target;

            if (GUILayout.Button("Copy ArrangeGameDataJson to clipboard"))
            {
                GUIUtility.systemCopyBuffer = instance.MatchGameDataJson();
            }
        }
    }
#endif
}

