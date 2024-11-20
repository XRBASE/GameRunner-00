using System;
using MathBuddy.Flags;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class VisibleWithEnum<T> : VisibleWithEnum where T : Enum
{
    protected override bool HasPrivilege {
        get { return _check.HasPrivilege; }
    }
    
    [SerializeField] protected T _required;
    [SerializeField] protected FlagExtensions.FlagPositive _checkFor;
    [SerializeField] protected FlagExtensions.CheckType _checkType;

    private EnumToggle<T> _check;
    private bool _subscribed = false;

    /// <summary>
    /// Subscribe the check value method to any callback that provides new values, when this object should re-evaluate it's visibility state.
    /// </summary>
    protected void SubscribeCheckValue(T current, ref Action<T> checkValueAction)
    {
        _check = new EnumToggle<T>(_required, _checkFor, _checkType, ref checkValueAction, gameObject);
        _check.onValueChanged += ChangeVisible;
        
        _check.CheckValue(current, true);
        _subscribed = true;
    }

    protected void OnDestroy()
    {
        if (!_subscribed)
            return;
        
        _check.onValueChanged -= ChangeVisible;
    }

    /// <summary>
    /// Called when visibility state changed.
    /// </summary>
    /// <param name="isVisible">visible state of the object.</param>
    protected void ChangeVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }
}

/// <summary>
/// Generic version of the class to write a custom editor for all instances of this class, which show their local result.
/// </summary>
public abstract class VisibleWithEnum : MonoBehaviour
{
    protected abstract bool HasPrivilege { get; }

#if UNITY_EDITOR
    [CustomEditor(typeof(VisibleWithEnum), true)]
    private class VisibleWithEnumEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            if (Application.isPlaying) {
                EditorGUI.BeginDisabledGroup (true);
                EditorGUILayout.Toggle("Check passed", ((VisibleWithEnum)target).HasPrivilege);
                EditorGUI.EndDisabledGroup();    
            }
        } 
    }
#endif
}


