using Cohort.GameRunner.Input;
using Cohort.GameRunner.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonInteraction : MonoBehaviour {
    [SerializeField] private ObjIndicator _btnFeedback;
    
    [SerializeField] private BaseInteractable[] _interactables;

    private int _activeId = -1;
    private bool _hasInteractables;
    
    private void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InputManager.Instance.InteractInput.onBtnInteract += OnInteract;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
        if (!InputManager.Disposed)
            InputManager.Instance.InteractInput.onBtnInteract -= OnInteract;
    }

    private void OnInteract() {
        if (_activeId < 0)
            return;
        
        _interactables[_activeId].OnInteract();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        _interactables = FindObjectsOfType<BaseInteractable>(true);
        _hasInteractables = _interactables.Length > 0;
        _btnFeedback.SetActive(false);
    }

    void Update() {
        if (!_hasInteractables)
            return;

        bool found = false;
        if (_activeId >= 0) {
            if (_interactables[_activeId].interactable && _interactables[_activeId].InInteractRange) {
                _btnFeedback.transform.position = _interactables[_activeId].transform.position;
                found = true;
            }
            else {
                _activeId = -1;
            }
        }
        
        //TODO_COHORT: make sure that if interactables overlap it takes the camera angle to determine the most valid one.
        if (!found) {
            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].interactable && _interactables[i].InInteractRange) {
                    _btnFeedback.transform.position = _interactables[i].transform.position;
                    _activeId = i;
                
                    found = true;
                    break;
                }
            }
        }
        
        _btnFeedback.SetActive(found);
    }
}
