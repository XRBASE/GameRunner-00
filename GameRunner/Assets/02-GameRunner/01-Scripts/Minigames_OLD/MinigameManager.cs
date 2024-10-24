using System;
using Cohort.Patterns;
using Cohort.Tools.Timers;
using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MinigameManager : Singleton<MinigameManager> {
    public Action<float> onMinigameFinished;
    
    private int _scoreMultiplier = 1;

    private MinigameTimer _timer;
    private MinigameInteractable[] _interactables;
    private (MinigameInteractable interactable, MiniGameDescription desc) _current;

    protected override void Awake() {
        base.Awake();

        _timer = gameObject.AddComponent<MinigameTimer>();
        _timer.onTimerFinished += AddMinigame;
    }
    
    private void OnDestroy() {
        if (_interactables != null) {
            for (int i = 0; i < _interactables.Length; i++) {
                _interactables[i].onMinigameStart -= OnMinigameStart;
            }
        }
    }

    public void OnActivityStart(int scoreFactor) {
        _interactables = FindObjectsOfType<MinigameInteractable>();
        _scoreMultiplier = scoreFactor;
        for (int i = 0; i < _interactables.Length; i++) {
            _interactables[i].onMinigameStart += OnMinigameStart;
        }
        
        _timer.StartTimer();
    }
    
    public void OnActivityStop() {
        if (_current != default) {
            OnMinigameFinished(0f);
        }

        UILocator.Get<MinigameUILog>().Clear();
        
        _timer.StopTimer();
    }

    private void AddMinigame() {
        Debug.LogError("adding minigame");
        
        int i = Random.Range(0, _interactables.Length);
        int c = 0;
        while (c <= _interactables.Length) {
            i = (i + 1) % _interactables.Length;
            
            if (!_interactables[i].HasMinigame) {
                break;
            }
            
            c++;
            if (c >= _interactables.Length) {
                return;
            }
        }
        
        Debug.LogError($"{_interactables[i].Description} has minigame");
        _interactables[i].ActivateMinigame();
    }

    private void OnMinigameStart(MiniGameDescription desc, MinigameInteractable interactable) {

        _current.desc = desc;
        desc._state = MiniGameDescription.State.InGame;
        _current.interactable = interactable;
        SceneManager.LoadScene(_current.desc.sceneName, LoadSceneMode.Additive);
    }

    private void OnMinigameFinished(float completed) {
        _current.desc._state = (completed > 0.01f)? MiniGameDescription.State.Completed : MiniGameDescription.State.Failed;
        SceneManager.UnloadSceneAsync(_current.desc.sceneName);
        
        _current.interactable.UnsetMinigame();
        _current.interactable.Deactivate();
        _current = default;
        
        onMinigameFinished?.Invoke(completed);
    }

    public void SetupMinigame(Minigame game) {
        game.Initialize(_current.desc.data, _scoreMultiplier, OnMinigameFinished);
    }w
}
