using UnityEngine;
[CreateAssetMenu(fileName = "ArrangeData", menuName = "Cohort/ArrangeData")]
public class ArrangeData : ScriptableObject
{
    public string UID;
    public string text;
    public Sprite sprite;
    

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
