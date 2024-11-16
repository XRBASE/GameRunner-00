using System;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using Cohort.UI.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[DefaultExecutionOrder(1)] //After LearningInteractable 
public class LearningManager : Singleton<LearningManager> {
    public const float MIN_TIME = 3f;
    public const float MAX_TIME = 7f;
    
    public LearningDescription this[int index] {
        get { return _settings.learnings[index]; }
    }

    public bool LearningsNetworked {
        get { return _settings.networked; }
    }

    [SerializeField] private LearningCycleDescription _settings;
    
    private int _scoreMultiplier;
    private LearningInteractable[] _interactables;
    
    //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
    private LearningDescription _currenOpenLearning;
    private LearningInteractable _currentOpenInteractable;

    [SerializeField] private List<float> _learningTimers;
    private float _refT = -1;
    private int _refActor = -1;
    
    private void Start() {
        ActivityLoader.Instance.onActivityStart += OnActivityStart;
        ActivityLoader.Instance.onActivityStop += OnActivityStop;

        _learningTimers = new List<float>();
        TimeManager.Instance.onRefTimeReset += OnTimeReset;
        
        if (_settings.networked) {
            Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
            Network.Local.Callbacks.onRoomPropertiesChanged += OnPropsChanged;
        }
    }

    private void OnDestroy() {
        ResetLearnings();
        
        ActivityLoader.Instance.onActivityStart -= OnActivityStart;
        ActivityLoader.Instance.onActivityStop -= OnActivityStop;

        if (_settings.networked) {
            Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
            Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropsChanged;
        }
    }

    private void FixedUpdate() {
        if (_learningTimers.Count > 0 && _learningTimers[0] > 0 && _learningTimers[0] <= TimeManager.Instance.RefTime) {
            OnTimerFin();
        }
    }

    private void InitTimerNetworkData() {
        Hashtable changes = new Hashtable();
        changes.Add(GetTimerKey(), -1f);
        changes.Add(GetActorKey(), -1);
        Debug.LogWarning($"Init timer network data");
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes); 
    }
    
    private void PushNewTimer() {
        if (_learningTimers.Count > 0)
            return;
        
        float t = Random.Range(MIN_TIME, MAX_TIME);
        t = TimeManager.Instance.RefTime + t;
        
        if (!_settings.networked) {
            _refActor = Player.Local.ActorNumber;
            _learningTimers.Add(t);
            return;
        }
        
        Hashtable changes = new Hashtable();
        changes.Add(GetTimerKey(), t);
        changes.Add(GetActorKey(), Player.Local.ActorNumber);
        
        Hashtable exp = new Hashtable();
        exp.Add(GetTimerKey(), _refT);
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes, exp); 
    }

    private void OnTimeReset() {
        for (int i = 0; i < _learningTimers.Count; i++) {
            _learningTimers[i] %= TimeManager.RESET_VALUE;
        }
    }

    private void OnTimerFin() {
        _learningTimers.RemoveAt(0);
        Debug.LogError($"Timer fin actor {_refActor}");

        if (_refActor == Player.Local.ActorNumber) {
            ActivateLearning();
        }
        
        if (ActivityLoader.Instance.InActivity) {
            PushNewTimer();
        }
    }

    private void ResetLearnings() {
        for (int i = 0; i < _settings.learnings.Length; i++) {
            _settings.learnings[i].state = LearningDescription.State.Open;
        }
    }

    private void OnActivityStart() {
        _scoreMultiplier = ActivityLoader.Instance.Activity.ScoreMultiplier;
        _interactables = FindObjectsOfType<LearningInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();
        
        if (_settings.useTimer) {
            if (_learningTimers.Count == 0)
                PushNewTimer();
        }
        else if (!AnyLearningActive()) {
            ActivateLearning();
        }

        if (!_settings.networked) {
            HighscoreTracker.Instance.ClearLocalScore();
        }
    }
    
    private void OnActivityStop() {
        _learningTimers.Clear();
        _refT = -1;
        
        ResetLearnings();
    }

    private bool AnyLearningActive() {
        for (int i = 0; i < _interactables.Length; i++) {
            if (_interactables[i].HasLearning) {
                return true;
            }
        }

        return false;
    }

    

    private void ActivateLearning() {
        if (TryGetNextLearning(out LearningDescription learning) &&
            TryGetInteractable(learning, out LearningInteractable interactable)) {
            
            interactable.SetLearning(learning);
        }
    }
    
    private bool TryGetNextLearning(out LearningDescription learning) {
        int learningId;
        if (_settings.linear) {
            learningId = -1;
            
            for (int i = 0; i < _settings.learnings.Length; i++) {
                if (_settings.learnings[i].state == LearningDescription.State.Open) {
                    learningId = i;
                    break;
                }
            }

            if (learningId == -1) {
                throw new Exception("All learnings finished!");
            }
        }
        else {
            learningId = Random.Range(0, _settings.learnings.Length);
            bool found = false;
            int check = 0;
            for (int i = 0; i < _settings.learnings.Length; i++) {
                check = (learningId + i) % _settings.learnings.Length;

                if (_settings.learnings[check].state == LearningDescription.State.Open) {
                    found = true;
                    learningId = check;
                    break;
                }
            }

            if (!found) {
                throw new Exception("All learnings finished!");
            }
        }
        
        learning = _settings.learnings[learningId];
        return true;
    }
    
    private bool TryGetInteractable(LearningDescription learning, out LearningInteractable interactable) {
        List<LearningInteractable> options = new List<LearningInteractable>();
        for (int iL = 0; iL < learning.locations.Length; iL++) {
            for (int iI = 0; iI < _interactables.Length; iI++) {
                if (_interactables[iI].Identifier == learning.locations[iL]) {
                    options.Add(_interactables[iI]);
                    break;
                }
            }
        }

        if (options.Count == 0) {
            interactable = null;
            return false;
        }

        int optionId = Random.Range(0,learning.locations.Length);
        int check;
        bool found = false;
        
        for (int i = 0; i < options.Count; i++) {
            check = (optionId + i) % options.Count;
            if (!options[check].HasLearning) {
                optionId = check;
                found = true;
                break;
            }
        }

        if (!found) {
            Debug.LogWarning($"No available interactable for learning {learning.index}!");
            
            interactable = null;
            return false;
        }

        interactable = options[optionId];
        return true;
    }

    public void OnLearningStart(LearningDescription learning, LearningInteractable interactable) {
        _currenOpenLearning = learning;
        _currentOpenInteractable = interactable;
        
        _currenOpenLearning.state = LearningDescription.State.Active;
        SceneManager.LoadScene(_currenOpenLearning.sceneName, LoadSceneMode.Additive);
    }

    private void OnLearningFinished(float scorePercentage) {
        LearningDescription.State s = (scorePercentage > 0.001f)
            ? LearningDescription.State.Completed
            : LearningDescription.State.Failed;
        
        if (_settings.complete) {
            _currenOpenLearning.state = s;
        }
        else {
            _currenOpenLearning.state = LearningDescription.State.Open;
        }

        if (_settings.networked) {
            PushLearningState(_currenOpenLearning);
        }
        
        
        _currenOpenLearning.log.CheckLogItem(s);
        _currenOpenLearning.log = null;
        
        HighscoreTracker.Instance.OnLearningFinished(scorePercentage);
        
        SceneManager.UnloadSceneAsync(_currenOpenLearning.sceneName);
        
        //TODO_COHORT: fix the double call thingie?
        _currentOpenInteractable.Deactivate();
        //_currentOpenInteractable.SetLearningLocal(-1);

        _currenOpenLearning = null;
        _currentOpenInteractable = null;
        
        if (!_settings.useTimer) {
            ActivateLearning();
        }
    }

    private void PushLearningState(LearningDescription l) {
        Hashtable changes = new Hashtable();
        changes.Add(GetLearningStateKey(l.index), l.state);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void SetLearningState(int learningId, LearningDescription.State state) {
        _settings.learnings[learningId].state = state;
    }

    public void SetLearningLog(LearningDescription learning, LearningInteractable interactable) {
        learning.log = UILocator.Get<LearningLogUI>().CreateLogEntry(learning.actionDescription, interactable.LocationDescription);
    }
    
    public void RemoveLearningLog(LearningDescription learning) {
        if (learning.log != null) {
            learning.log.CheckLogItem(LearningDescription.State.Failed);
            learning.log = null;
        }
    }
    
    public void InitializeLearning(Learning learning) {
        learning.Initialize(_currenOpenLearning.data, _scoreMultiplier, OnLearningFinished);
    }
    
    private void OnJoinedRoom() {
        OnPropsChanged(Network.Local.Client.CurrentRoom.CustomProperties);

        string key = GetTimerKey();
        if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) ||
            (TimeManager.Instance.RefTime - (float)Network.Local.Client.CurrentRoom.CustomProperties[key]) > MAX_TIME) {
            InitTimerNetworkData();
        }
    }

    private void OnPropsChanged(Hashtable changes) {
        string key;
        for (int i = 0; i < _settings.learnings.Length; i++) {
            key = GetLearningStateKey(i);

            if (changes.ContainsKey(key)) {
                LearningDescription.State s;
                if (changes[key] == null) {
                    s = LearningDescription.State.Open;
                }
                else {
                    s = (LearningDescription.State)changes[key];
                }
                
                SetLearningState(i, s);
            }
        }
        
        key = GetTimerKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] != null) {
                _refT = (float)changes[key];
                _refActor = (int)changes[GetActorKey()];

                if (_refT < 0) {
                    if (ActivityLoader.Instance.InActivity)
                        PushNewTimer();
                    else
                        _learningTimers.Clear();
                }
                else {
                    Debug.LogError($"Add timer {_refT - TimeManager.Instance.RefTime} actor: {_refActor}");
                    _learningTimers.Add(_refT);
                }
            }
            else {
                InitTimerNetworkData();
            }
        }
    }
    
    private string GetLearningStateKey(int learningId) {
        return Keys.Concatenate(
            Keys.Concatenate(Keys.Room.Learning, Keys.Learning.State), 
            learningId.ToString());
    }
    
    private string GetTimerKey() {
        return Keys.Concatenate(
            Keys.Concatenate(Keys.Room.Learning, Keys.Learning.ManagerTimer));
    }
    
    private string GetActorKey() {
        return Keys.Concatenate(
            Keys.Concatenate(Keys.Room.Learning, Keys.Learning.ManagerActor));
    }
}
