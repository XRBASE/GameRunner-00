using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MatchPairData", menuName = "Cohort/MatchPairData", order = 2)]
public class MatchPairData : ScriptableObject
{
    public enum MatchType
    {
        Image,
        Text
    }

    public string UID;

    public string originLabelText;
    public MatchType originMatchType;
    public Sprite originSprite;
    public string originText;

    public string targetLabelText;
    public MatchType targetMatchType;
    public Sprite targetSprite;
    public string targetText;


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
#if UNITY_EDITOR
[CustomEditor(typeof(MatchPairData))]
public class MatchPairDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MatchPairData matchPairData = (MatchPairData) target;

        GUILayout.Label("UID:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(matchPairData.UID, EditorStyles.wordWrappedLabel);
        matchPairData.originLabelText = EditorGUILayout.TextField("OriginLabelText", matchPairData.originLabelText);
        matchPairData.originMatchType =
            (MatchPairData.MatchType) EditorGUILayout.EnumPopup("OriginMatchType", matchPairData.originMatchType);

        if (matchPairData.originMatchType == MatchPairData.MatchType.Image)
        {
            if (matchPairData.originSprite != null)
            {
                GUILayout.Label("Sprite Preview:");
                GUILayout.Box(matchPairData.originSprite.texture, GUILayout.Width(100), GUILayout.Height(100));
            }
            else
            {
                GUILayout.Label("No sprite assigned.");
            }

            matchPairData.originSprite =
                (Sprite) EditorGUILayout.ObjectField("Sprite Field", matchPairData.originSprite, typeof(Sprite), false);
        }
        else if (matchPairData.originMatchType == MatchPairData.MatchType.Text)
        {
            matchPairData.originText = EditorGUILayout.TextField("Text", matchPairData.originText);
        }

        GUILayout.Space(20);
        matchPairData.targetLabelText = EditorGUILayout.TextField("TargetLabelText", matchPairData.targetLabelText);
        matchPairData.targetMatchType =
            (MatchPairData.MatchType) EditorGUILayout.EnumPopup("TargetMatchType", matchPairData.targetMatchType);

        if (matchPairData.targetMatchType == MatchPairData.MatchType.Image)
        {
            if (matchPairData.targetSprite != null)
            {
                GUILayout.Label("Sprite Preview:");
                GUILayout.Box(matchPairData.targetSprite.texture, GUILayout.Width(100), GUILayout.Height(100));
            }
            else
            {
                GUILayout.Label("No sprite assigned.");
            }

            matchPairData.targetSprite =
                (Sprite) EditorGUILayout.ObjectField("Sprite Field", matchPairData.targetSprite, typeof(Sprite), false);
        }
        else if (matchPairData.targetMatchType == MatchPairData.MatchType.Text)
        {
            matchPairData.targetText = EditorGUILayout.TextField("Text", matchPairData.targetText);
        }

        EditorUtility.SetDirty(matchPairData);
        serializedObject.ApplyModifiedProperties();
    }
}
#endif