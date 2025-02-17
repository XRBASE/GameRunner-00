using Base.Ravel.TranslateAttributes;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CopyTransformOf : MonoBehaviour
{
    //Attributes to copy
    [SerializeField] private TransformAttribute _attributes;
    
    //Space in/of which the values are applied/copied
    [SerializeField] private TransformSpace _copySpace;
    [SerializeField] private TransformSpace _applySpace;
    
    //Transform field to copy (if player is not copied).
    [SerializeField, HideInInspector] private Transform _toCopy;
    //Axes (per attribute) that will be copied.
    [FormerlySerializedAs("axes")]
    [SerializeField, HideInInspector] private bool[] _axes = new bool[9];

    //Should copy be exectuted in the update of the script, or is it called manually.
    [SerializeField] private bool _active = true;
    
    private void Awake()
    {
        SetActive(_toCopy != null);
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    private void Update()
    {
        //only copy if there's something to copy.
        if (_active && _attributes != TransformAttribute.None) {
            CopyValues();
        }
    }

    /// <summary>
    /// Get axis value of a given attribute in the boolean array.
    /// </summary>
    /// <param name="attribute">index of the attribute in enum list.</param>
    /// <param name="axis">axis index x=0, y=1, z=2.</param>
    private bool GetAxis(int attribute, int axis)
    {
        return _axes[attribute * 3 + axis];
    }

    /// <summary>
    /// Copy's the values of copy according to the settings of this script.
    /// </summary>
    public void CopyValues()
    {
        Vector3 value;
        if (_attributes.HasFlag(TransformAttribute.Position)) {
            value = FilterAttribute( 0,
                (_applySpace == TransformSpace.Self) ? transform.localPosition : transform.position,
                (_copySpace == TransformSpace.Self) ? _toCopy.localPosition : _toCopy.position);

            if (_applySpace == TransformSpace.Self)
                transform.localPosition = value;
            else
                transform.position = value;
        }
        if (_attributes.HasFlag(TransformAttribute.Rotation)) {
            value = FilterAttribute( 1,
                (_applySpace == TransformSpace.Self) ? transform.localRotation.eulerAngles : transform.rotation.eulerAngles,
                (_copySpace == TransformSpace.Self) ? _toCopy.localRotation.eulerAngles : _toCopy.rotation.eulerAngles);

            if (_applySpace == TransformSpace.Self)
                transform.localRotation = Quaternion.Euler(value);
            else
                transform.rotation = Quaternion.Euler(value);
        }
        if (_attributes.HasFlag(TransformAttribute.Scale)) {
            value = FilterAttribute( 2,
                (_applySpace == TransformSpace.Self) ? transform.localScale : transform.lossyScale,
                (_copySpace == TransformSpace.Self) ? _toCopy.localScale : _toCopy.lossyScale);

            if (_applySpace == TransformSpace.Self)
                transform.localScale = value;
            else
                transform.localScale = ConvertLossyToLocal(value);
        }
    }
    
    /// <summary>
    /// Returns vector 3 value, of which only the set booleans contain the value of Copy. All other values are taken from the apply value.
    /// </summary>
    /// <param name="attributeIndex">Index of the attribute from which to take the booleans (0 = pos, 1 = rot, 2 = scale).</param>
    /// <param name="applyPosition">position of object on which the values are applied.</param>
    /// <param name="copyPosition">position of the object that is being copied.</param>
    /// <returns>Apply's vector3 value in which checked boolean axes have been replaced with copy's values.</returns>
    private Vector3 FilterAttribute(int attributeIndex, Vector3 applyPosition, Vector3 copyPosition)
    {
        //retrieves first three bools from bool array, to check which values should be taken from apply and which from copy.
        Vector3 value;
        value.x = (GetAxis(attributeIndex, 0) ? copyPosition.x : applyPosition.x);
        value.y = (GetAxis(attributeIndex, 1) ? copyPosition.y : applyPosition.y);
        value.z = (GetAxis(attributeIndex, 2) ? copyPosition.z : applyPosition.z);
        
        return value;
    }

    /// <summary>
    /// Calculates local scale of this object, that will result in a lossy scale as given in parameters.
    /// </summary>
    private Vector3 ConvertLossyToLocal(Vector3 lossyScale)
    {
        if (transform.parent == null) {
            //if object has no parent, lossy scale is local scale.
            return lossyScale;
        }
        
        //if either value is 0, the result is 0
        //otherwise the result is the lossy scale divided by the lossy scale of the parent object.
        Vector3 parent = transform.parent.lossyScale;
        return new Vector3(
             (Mathf.Abs(lossyScale.x) < MathBuddy.FloatingPoints.LABDA || Mathf.Abs(parent.x) < MathBuddy.FloatingPoints.LABDA)? 0f : 
                 lossyScale.x / parent.x,
             (Mathf.Abs(lossyScale.y) < MathBuddy.FloatingPoints.LABDA || Mathf.Abs(parent.y) < MathBuddy.FloatingPoints.LABDA)? 0f : 
                lossyScale.y / parent.y,
             (Mathf.Abs(lossyScale.z) < MathBuddy.FloatingPoints.LABDA || Mathf.Abs(parent.z) < MathBuddy.FloatingPoints.LABDA)? 0f : 
                lossyScale.z / parent.z);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(CopyTransformOf))]
    private class MatchTransformOfEditor : Editor
    {
        //order of these attributes is dependent on the CopyAtttributes enum.
        private readonly string[] ATTRIBUTE_NAMES = new [] {"position", "rotation", "scale"};
        private readonly string[] AXIS_NAMES = new [] {"x", "y", "z"};
        
        private bool _dirty = false;
        
        public override void OnInspectorGUI()
        {
            CopyTransformOf instance = (CopyTransformOf) target;
            
            DrawDefaultInspector();
            _dirty = false;
            
            EditorGUI.BeginChangeCheck();
            instance._toCopy = EditorGUILayout.ObjectField("to copy", instance._toCopy, typeof(Transform), true) as Transform;
            
            GUILayout.Label("Axes to apply:");
            for (int i = 0; i < ATTRIBUTE_NAMES.Length; i++) {
                //checks index of name against the position in the CopyAtttributes enum (order should match).
                if (instance._attributes.HasFlag((TransformAttribute) (1 << i))) {
                    GUILayout.Label(ATTRIBUTE_NAMES[i]);
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < AXIS_NAMES.Length; j++) {
                        instance._axes[i * ATTRIBUTE_NAMES.Length + j] =
                            EditorGUILayout.Toggle(instance._axes[i * ATTRIBUTE_NAMES.Length + j], GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.EndHorizontal();
                }
            }
            
            if (EditorGUI.EndChangeCheck() || _dirty) {
                EditorUtility.SetDirty(instance);
            }
        }
    }
#endif
}
