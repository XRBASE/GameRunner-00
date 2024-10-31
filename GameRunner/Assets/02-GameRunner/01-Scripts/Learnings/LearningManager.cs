using System;
using System.Collections.Generic;
using System.Linq;
using Cohort.Patterns;
using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LearningManager : Singleton<LearningManager> {
    public const float MIN_TIME = 2;
    public const float MAX_TIME = 5;

    public LearningDescription this[int index] {
        get { return _settings.learnings[index]; }
    }

    public bool LearningsNetworked {
        get { return _settings.networked; }
    }

    [SerializeField] private LearningCycleDescription _settings;
    [SerializeField] private NetworkedTimer _timer;
    
    private int _scoreMultiplier;
    private LearningInteractable[] _interactables;

    private int _lastLearingIndex = -1;
    
    //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
    private LearningDescription _currenOpenLearning;
    private LearningInteractable _currentOpenInteractable;

    private void OnDestroy() {
        for (int i = 0; i < _settings.learnings.Length; i++) {
            _settings.learnings[i].state = LearningDescription.State.Open;
        }
    }

    public void OnActivityStart(int scoreMultiplier) {
        _scoreMultiplier = scoreMultiplier;
        _interactables = FindObjectsOfType<LearningInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();

        _timer.onFinish += OnTimerFinished;
        if (_settings.useTimer) {
            StartTimer();
        }
        else {
            ActivateLearning();
        }
    }

    public void OnActivityStop() {
        _timer.onFinish -= OnTimerFinished;
    }
    
    private void StartTimer() {
        float duration = Random.Range(MIN_TIME, MAX_TIME);
        _timer.StartTimer(duration);
    }

    private void OnTimerFinished() {
        ActivateLearning();

        if (_settings.useTimer) {
            StartTimer();
        }
    }

    private void ActivateLearning() {
        LearningDescription learning = GetNextLearning();
        LearningInteractable interactable = GetInteractable(learning);
        learning.log = UILocator.Get<LearningLogUI>().CreateLogEntry(learning.actionDescription, interactable.LocationDescription);
        
        interactable.SetLearning(learning);
    }
    
    private LearningDescription GetNextLearning() {
        int learningId;
        if (_settings.linear) {
            learningId = _lastLearingIndex + 1;
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
                throw new Exception("No available learnings anymore");
            }
        }
        
        return _settings.learnings[learningId];
    }
    
    private LearningInteractable GetInteractable(LearningDescription learning) {
        List<LearningInteractable> options = new List<LearningInteractable>();
        for (int iL = 0; iL < learning.locations.Length; iL++) {
            for (int iI = 0; iI < _interactables.Length; iI++) {
                if (_interactables[iI].Identifier == learning.locations[iL]) {
                    options.Add(_interactables[iI]);
                    break;
                }
            }
        }

        if (options.Count != learning.locations.Length) {
            Debug.LogError("Options do not match locations: This should not happen!");
        }

        int interactableId = learning.locations[Random.Range(0,learning.locations.Length)];
        int check;
        bool found = false;
        
        for (int i = 0; i < options.Count; i++) {
            check = (interactableId + i) % options.Count;
            if (!options[check].HasLearning) {
                interactableId = check;
                found = true;
                break;
            }
        }

        if (!found) {
            throw new Exception("No available interactables!");
        }

        return options[interactableId];
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
        
        _currenOpenLearning.log.CheckLogItem(s);
        _currenOpenLearning.log = null;
        
        SceneManager.UnloadSceneAsync(_currenOpenLearning.sceneName);
        
        //TODO_COHORT: fix the double call thingie?
        _currentOpenInteractable.ClearLearning();;
        _currentOpenInteractable.Deactivate();

        _currenOpenLearning = null;
        _currentOpenInteractable = null;
        
        if (!_settings.useTimer) {
            ActivateLearning();
        }
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
}
