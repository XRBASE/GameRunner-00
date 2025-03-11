using Cohort.GameRunner.Interaction;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Event based interactable for generic scene interaction purposes.
/// </summary>
public class Interactable : BaseInteractable {
    [Header("Cinematic")]
    [SerializeField, Tooltip("Activate event, that is called while the user is in the scene.")] 
    private UnityEvent activateCinematic;
    [SerializeField, Tooltip("Deactivate event, that is called while the user is in the scene.")] 
    private UnityEvent deactivateCinematic;
    
    [Header("Direct")]
    //presets the object state, without cinematic effects like camera focus
    [SerializeField, Tooltip("Activate event, that is called while the scene is being loaded.")]
    private UnityEvent activateDirect;
    [SerializeField, Tooltip("Deactivate event, that is called while the scene is being loaded.")]
    private UnityEvent deactivateDirect;
    
    public override void OnInteract() {
        //flip whatever value is right now.
        if (Value) {
            Deactivate();
        }
        else {
            Activate();
        }
    }

    protected override void ActivateLocal() {
        base.ActivateLocal();
        
        if (Initial) {
            activateDirect?.Invoke();
        }
        else {
            activateCinematic?.Invoke();
        }
    }

    protected override void DeactivateLocal() {
        base.DeactivateLocal();
        
        if (Initial) {
            deactivateDirect?.Invoke();
        }
        else {
            deactivateCinematic?.Invoke();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Interactable))]
    private class InteractableEditor : Editor {
        private Interactable _instance;

        private void OnEnable() {
            _instance = (Interactable)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("->> Copy cinematic to direct")) {
                if (_instance.activateCinematic != null) {
                    _instance.activateDirect = _instance.activateCinematic;
                }

                if (_instance.deactivateCinematic != null) {
                    _instance.deactivateDirect = _instance.deactivateCinematic;
                }
            }
            if (GUILayout.Button("<<- Copy direct to cinematic")) {
                if (_instance.activateDirect != null) {
                    _instance.activateCinematic = _instance.activateDirect;
                }

                if (_instance.deactivateDirect != null) {
                    _instance.deactivateCinematic = _instance.deactivateDirect;
                }
            }
        }
    }
#endif
}
