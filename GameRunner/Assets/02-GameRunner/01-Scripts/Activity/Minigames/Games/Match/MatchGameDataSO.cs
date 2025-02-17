using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Use this to easily copy a chosen configuration to be injected into the SceneConfiguration Minigame Data
/// </summary>
[CreateAssetMenu(fileName = "MatchGameDataSO", menuName = "Cohort/MatchGameData", order = 1)]
public class MatchGameDataSO : ScriptableObject
{
    public MatchGameData matchGameData;

    private string MatchGameDataJson()
    {
        matchGameData.chosenIds = matchGameData.matchPairs.Select(pair => pair.UID).ToList();
        return JsonUtility.ToJson(matchGameData);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MatchGameDataSO))]
    private class WordGameDataSoEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var instance = (MatchGameDataSO) target;

            if (GUILayout.Button("Copy WordGameDataJson to clipboard"))
            {
                GUIUtility.systemCopyBuffer = instance.MatchGameDataJson();
            }
        }
    }
#endif
}