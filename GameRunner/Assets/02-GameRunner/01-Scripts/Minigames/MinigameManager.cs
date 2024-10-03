using Cohort.Patterns;
using Cohort.Tools.Timers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MinigameManager : Singleton<MinigameManager> {
    [SerializeField] private MiniGameDescription[] _minigames;
    [SerializeField] private float _newGameTimeMin = 10f;
    [SerializeField] private float _newGameTimeMax = 20f;

    private Timer _newGameTimer;
    private MinigameInteractable[] _interactables;
    private (MinigameInteractable interactable, MiniGameDescription desc) _current;

    protected override void Awake() {
        base.Awake();
        
        _newGameTimer = new Timer(Random.Range(_newGameTimeMin, _newGameTimeMax), false, ActivateMinigame);
    }

    private void Start() {
        GameLoader.Instance.onActivityStart += OnPlayersReady;
        GameLoader.Instance.onActivityStop += OnActivityStop;
    }
    
    private void OnDestroy() {
        GameLoader.Instance.onActivityStart -= OnPlayersReady;
        GameLoader.Instance.onActivityStop -= OnActivityStop;
        
        for (int i = 0; i < _interactables.Length; i++) {
            _interactables[i].onMinigameStart -= OnMinigameStart;
        }
    }

    private void OnActivityStop() {
        if (_current != default) {
            OnMinigameFinished(false);
        }
        
        _newGameTimer.Stop();
        _newGameTimer.Reset();
    }

    private void OnPlayersReady() {
        _interactables = FindObjectsOfType<MinigameInteractable>();
        for (int i = 0; i < _interactables.Length; i++) {
            _interactables[i].onMinigameStart += OnMinigameStart;
        }
        
        _newGameTimer.Start();
        Debug.Log($"Start timer {_newGameTimer.duration} (initial)");
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
        
        int random = Random.Range(0, _interactables.Length);
        int i = 0;
        int c = 0;
        while (random > 0) {
            if (!_interactables[i].HasMinigame) {
                random--;
            }
            else {
                c++;

                if (c >= _interactables.Length) {
                    Debug.Log("Skipped new minigame, system is at capacity");
                    return;
                }
            }

            i = (i + 1) % _interactables.Length;
        }
        
        _interactables[i].ActivateMinigame(minigameId);
    }

    private void OnMinigameStart(int index, MinigameInteractable interactable) {

        _current.desc = _minigames[index];
        _current.interactable = interactable;
        SceneManager.LoadScene(_current.desc.assetRef, LoadSceneMode.Additive);
    }

    private void OnMinigameFinished(bool completed) {
        Debug.LogError($"Minigame has been {(completed? "completed" : "failed")}");
        
        SceneManager.UnloadSceneAsync(_current.desc.assetRef);
        
        _current.interactable.DeactivateMinigame();
        _current.interactable.Deactivate();
        _current = default;
    }

    public void SetupMinigame(Minigame game) {
        game.Initialize(_current.desc.data, OnMinigameFinished);
    }
}
