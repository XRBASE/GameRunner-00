using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace Cohort.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ReadOnlyAttribute : PropertyAttribute { }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class NumberToIndex : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //disable GUI, draw property as not enabled and re-enable GUI
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
    
    [CustomPropertyDrawer(typeof(NumberToIndex))]
    public class NumberToIndexPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //disable GUI, draw property as not enabled and re-enable GUI
            if (property.propertyType != SerializedPropertyType.Integer) {
                EditorGUI.HelpBox(position, "Cannot use NormieNumber on non int values", MessageType.Error);
                return;
            }
            
            int value = property.intValue + 1;
            value = EditorGUI.IntField(position, label, value);
            property.intValue = value - 1;
        }
    }
#endif
}