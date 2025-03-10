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

    private void Reset()
    {
        AssignNewUID();
    }

    public void AssignNewUID()
    {
        UID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
