using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchGameDataSO", menuName = "Cohort/MatchGameData", order = 1)]
public class MatchGameDataSO : ScriptableObject
{
  public MatchGameData matchGameData;
  
  public string MatchGameDataJson()
  {
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
