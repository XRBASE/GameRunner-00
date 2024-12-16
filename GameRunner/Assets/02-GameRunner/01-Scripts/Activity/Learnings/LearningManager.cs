using System;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Input;
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
    
    public LearningCycleDescription Setting { get; set; }
    
    public LearningDescription this[int index] {
        get { return Setting.learnings[index]; }
    }

    public LearningDescription Current {
        get { return _currenOpenLearning; }
    }

    public int ScoreMultiplier {
        get { return _scoreMultiplier; }
    }

    public bool LearningsNetworked {
        get { return Setting.networked; }
    }

    public Action<float> onLearningFinished;
    public Action onScoreReset;
    
    [SerializeField] private List<float> _learningTimers;
    
    private int _scoreMultiplier;
    private LearningInteractable[] _interactables;
    
    //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
    private LearningDescription _currenOpenLearning;
    private LearningInteractable _currentOpenInteractable;
    
    private float _refTime = -1;
    private int _refActor = -1;
    private bool _inActivity = false;
    
    private void Start() {
        _learningTimers = new List<float>();
    }

    private void OnDestroy() {
        if (Setting != null && Setting.networked) {
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
        
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes); 
    }
    
    private void PushNewTimer() {
        if (_learningTimers.Count > 0)
            return;
        
        float t = Random.Range(MIN_TIME, MAX_TIME);
        t = TimeManager.Instance.RefTime + t;
        
        if (!Setting.networked) {
            _refActor = Player.Local.ActorNumber;
            _learningTimers.Add(t);
            return;
        }
        
        Hashtable changes = new Hashtable();
        changes.Add(GetTimerKey(), t);
        changes.Add(GetActorKey(), Player.Local.ActorNumber);
        
        Hashtable exp = new Hashtable();
        exp.Add(GetTimerKey(), _refTime);
        
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
        
        if (_inActivity) {
            PushNewTimer();
        }
    }

    private void ResetLearnings() {
        if (Setting != null) {
            for (int i = 0; i < Setting.learnings.Length; i++) {
                Setting.learnings[i].Reset();
            }
        }
    }
    
    public void OnActivityStart(int scoreMultiplier) {
        _inActivity = true;
        
        TimeManager.Instance.onRefTimeReset += OnTimeReset;
        
        if (Setting.networked) {
            Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
            Network.Local.Callbacks.onRoomPropertiesChanged += OnPropsChanged;

            if (Network.Local.Client.InRoom) {
                OnJoinedRoom();
            }
        }
        
        _scoreMultiplier = scoreMultiplier;
        _interactables = FindObjectsOfType<LearningInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();
        
        if (Setting.useTimer) {
            PushNewTimer();
        }
        else if (!AnyLearningActive()) {
            ActivateLearning();
        }

        if (!Setting.networked) {
            onScoreReset?.Invoke();
        }
    }
    
    public void OnActivityStop() {
        _inActivity = false;
        UILocator.Get<LearningLogUI>().ClearLog();
        
        _learningTimers.Clear();
        _refTime = -1;
        
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
        if (Setting.linear) {
            learningId = -1;
            
            for (int i = 0; i < Setting.learnings.Length; i++) {
                if (Setting.learnings[i].LearningState == LearningDescription.State.Open) {
                    learningId = i;
                    break;
                }
            }

            if (learningId == -1) {
                throw new Exception("All learnings finished!");
            }
        }
        else {
            learningId = Random.Range(0, Setting.learnings.Length);
            bool found = false;
            int check = 0;
            for (int i = 0; i < Setting.learnings.Length; i++) {
                check = (learningId + i) % Setting.learnings.Length;

                if (Setting.learnings[check].LearningState == LearningDescription.State.Open) {
                    found = true;
                    learningId = check;
                    break;
                }
            }

            if (!found) {
                throw new Exception("All learnings finished!");
            }
        }
        
        learning = Setting.learnings[learningId];
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
        InputManager.Instance.SetLearningInput();
        _currenOpenLearning = learning;
        _currentOpenInteractable = interactable;
        
        _currenOpenLearning.SetState(LearningDescription.State.Active, false);
        SceneManager.LoadScene(_currenOpenLearning.sceneName, LoadSceneMode.Additive);
    }

    private void OnLearningFinished(float scorePercentage) {
        InputManager.Instance.SetGameInput();
        LearningDescription.State s = (scorePercentage > 0.001f)
            ? LearningDescription.State.Completed
            : LearningDescription.State.Failed;
        
        if (Setting.complete) {
            _currenOpenLearning.SetState(s, false);
        }
        else {
            _currenOpenLearning.SetState(LearningDescription.State.Open, false, true);
        }

        if (Setting.networked) {
            PushLearningState(_currenOpenLearning);
        }
        
        onLearningFinished?.Invoke(scorePercentage);
        
        _currenOpenLearning.log.CheckLogItem(s);
        _currenOpenLearning.log = null;
        
        SceneManager.UnloadSceneAsync(_currenOpenLearning.sceneName);
        
        //TODO_COHORT: fix the double call thingie?
        _currentOpenInteractable.Deactivate();
        //_currentOpenInteractable.SetLearningLocal(-1);

        _currenOpenLearning = null;
        _currentOpenInteractable = null;
        
        if (!Setting.useTimer) {
            ActivateLearning();
        }
    }

    private void PushLearningState(LearningDescription l) {
        Hashtable changes = new Hashtable();
        changes.Add(GetLearningStateKey(l.index), l.LearningState);

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }

    private void SetLearningState(int learningId, LearningDescription.State state, bool init) {
        Setting.learnings[learningId].SetState(state, init);
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
        OnPropsChanged(Network.Local.Client.CurrentRoom.CustomProperties, true);

        string key = GetTimerKey();
        if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) ||
            (TimeManager.Instance.RefTime - (float)Network.Local.Client.CurrentRoom.CustomProperties[key]) > MAX_TIME) {
            InitTimerNetworkData();
        }
    }

    private void OnPropsChanged(Hashtable changes) {
        OnPropsChanged(changes, false);
    }

    private void OnPropsChanged(Hashtable changes, bool init) {
        string key;
        for (int i = 0; i < Setting.learnings.Length; i++) {
            key = GetLearningStateKey(i);

            if (changes.ContainsKey(key)) {
                LearningDescription.State s;
                if (changes[key] == null) {
                    s = LearningDescription.State.Open;
                }
                else {
                    s = (LearningDescription.State)changes[key];
                }
                
                SetLearningState(i, s, init);
            }
        }

        if (Setting.useTimer) {
            key = GetTimerKey();
            if (changes.ContainsKey(key)) {
                if (changes[key] != null) {
                    _refTime = (float)changes[key];
                    _refActor = (int)changes[GetActorKey()];

                    if (_refTime < 0) {
                        if (_inActivity)
                            PushNewTimer();
                        else
                            _learningTimers.Clear();
                    }
                    else {
                        _learningTimers.Add(_refTime);
                    }
                }
                else {
                    InitTimerNetworkData();
                }
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
