using Cohort.GameRunner.InformationPoints;
using Cohort.GameRunner.Input;
using Cohort.Patterns;
using UnityEngine.SceneManagement;

public class InfoManager : Singleton<InfoManager>
{
    private InfoDescription _curDesc;
    private InfoInteractable _curInteractable;

    public void ActivateInfo(InfoDescription desc, InfoInteractable interactable) {
        _curDesc = desc;
        _curInteractable = interactable;
        
        InputManager.Instance.SetMinigameInput();
        SceneManager.LoadScene(_curDesc.sceneName, LoadSceneMode.Additive);
    }

    public void InitializeInfo(InfoViewer viewer) {
        viewer.Initialize(_curDesc.json, OnInfoClosed);
    }

    private void OnInfoClosed() {
        InputManager.Instance.SetGameInput();
        
        SceneManager.UnloadSceneAsync(_curDesc.sceneName);
        _curInteractable.Deactivate();
    }
}
