using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Popup", menuName = "Cohort/Popup")]
public class Popup : ScriptableObject
{
    public string UID;
    public string text;
    public float timestamp;
    public bool dummy;
    
    public void AssignNewUID()
    {
        UID = System.Guid.NewGuid().ToString();
    }
    
    
}
#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(Popup))]
public class MyScriptableObjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Popup myScriptable = (Popup)target;
        if (GUILayout.Button("Generate New UID"))
        {
            myScriptable.AssignNewUID();
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(myScriptable);
        }
    }
}
#endif
