using System;
using Cohort.Patterns;
using Cohort.Tools.Timers;
using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MinigameManager : Singleton<MinigameManager> {
    public Action<float> onMinigameFinished;
    
    [SerializeField] private MiniGameDescription[] _minigames;
    [SerializeField] private float _newGameTimeMin = 10f;
    [SerializeField] private float _newGameTimeMax = 20f;

    private int _scoreMultiplier = 1;
    private Timer _newGameTimer;
    private MinigameInteractable[] _interactables;
    private (MinigameInteractable interactable, MiniGameDescription desc) _current;

    protected override void Awake() {
        base.Awake();

        for (int i = 0; i < _minigames.Length; i++) {
            _minigames[i]._index = i;
        }
        
        _newGameTimer = new Timer(Random.Range(_newGameTimeMin, _newGameTimeMax), false, ActivateMinigame);
    }
    
    private void OnDestroy() {
        for (int i = 0; i < _interactables.Length; i++) {
            _interactables[i].onMinigameStart -= OnMinigameStart;
        }
    }

    public void StartMinigames(int scoreFactor) {
        _interactables = FindObjectsOfType<MinigameInteractable>();
        _scoreMultiplier = scoreFactor;
        for (int i = 0; i < _interactables.Length; i++) {
            _interactables[i].onMinigameStart += OnMinigameStart;
        }
        
        _newGameTimer.Start();
        Debug.Log($"Start timer {_newGameTimer.duration} (initial)");
    }
    
    public void StopMinigames() {
        if (_current != default) {
            OnMinigameFinished(0f);
        }

        UILocator.Get<MinigameUILog>().Clear();
        
        _newGameTimer.Stop();
        _newGameTimer.Reset();
    }

    private void SetUpTimer() {
        _newGameTimer.Reset();
        _newGameTimer.duration = Random.Range(_newGameTimeMin, _newGameTimeMax);
        Debug.Log($"Start timer {_newGameTimer.duration}");
        
        _newGameTimer.Start();
    }

    private void ActivateMinigame() {
        
        Debug.Log("Set up minigame");
        
        SetUpTimer();
        int minigameId = Random.Range(0, _minigames.Length);
        
        int i = Random.Range(0, _interactables.Length);
        int c = 0;
        while (c <= _interactables.Length) {
            i = (i + 1) % _interactables.Length;
            
            if (!_interactables[i].HasMinigame) {
                Debug.LogWarning($"I:{i}\t True\t C:{_interactables.Length - c}");
                break;
            }
            else {
                c++;
                
                if (c >= _interactables.Length) {
                    Debug.LogWarning($"I:{i}\t False\t C:{_interactables.Length - c}, Skipped new minigame, system is at capacity");
                    return;
                }
                else {
                    Debug.LogWarning($"I:{i}\t False\t C:{_interactables.Length - c}");
                }
            }
        }
        
        Debug.LogError($"{_interactables[i].Description} has minigame");
        _interactables[i].SetMinigame(_minigames[minigameId]);
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
    }

    public MiniGameDescription GetDescription(int index) {
        return _minigames[index];
    }
}
