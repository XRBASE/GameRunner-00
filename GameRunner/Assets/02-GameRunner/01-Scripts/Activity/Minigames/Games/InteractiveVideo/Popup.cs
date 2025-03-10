using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Popup", menuName = "Cohort/Popup")]
public class Popup : ScriptableObject
{
    public string UID;
    public string text;
    public float timestamp;
    public bool dummy;
    
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(UID))
        {
            AssignNewUID();
        }
    }
    
    public void AssignNewUID()
    {
        Debug.LogError(UID);
        Debug.LogError("Assigning new UID");
        UID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}
