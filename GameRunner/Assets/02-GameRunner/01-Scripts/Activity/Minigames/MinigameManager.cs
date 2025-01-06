using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

using Cohort.GameRunner.Input;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using Cohort.UI.Generic;

using Random = UnityEngine.Random;

namespace Cohort.GameRunner.Minigames {
    [DefaultExecutionOrder(1)] //After LearningInteractable
    public class MinigameManager : Singleton<MinigameManager> {
        public const float FAILURE_THRESHOLD = 0.25f;
        public const float AMAZING_THRESHOLD = 0.75f;

        private const float MIN_TIME = 3f;
        private const float MAX_TIME = 7f;

        public MinigameCycleDescription Setting { get; set; }

        public MinigameDescription this[int index] {
            get { return Setting.minigames[index]; }
        }

        public MinigameDescription Current {
            get { return _currenMinigameDescription; }
        }

        public int ScoreMultiplier {
            get { return _scoreMultiplier; }
        }

        public bool LearningsNetworked {
            get { return Setting.networked; }
        }

        public Action<int> onAllMinigamesFinished;
        public Action<float> onMinigameFinished;
        public Action onScoreReset;

        [SerializeField] private List<float> _minigameTimers;

        private int _scoreMultiplier;
        private MinigameInteractable[] _interactables;

        //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
        private MinigameDescription _currenMinigameDescription;
        private Minigame _currentMinigame;
        private MinigameInteractable _currentInteractable;

        private float _refTime = -1;
        private int _refActor = -1;
        private bool _inActivity = false;

        private void Start() {
            _minigameTimers = new List<float>();
        }

        private void OnDestroy() {
            if (Setting != null && Setting.networked) {
                Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
                Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropsChanged;
                Network.Local.Callbacks.onService -= UpdateNetwork;
            }
        }

        private void UpdateNetwork() {
            if (_minigameTimers.Count > 0 && _minigameTimers[0] > 0 &&
                _minigameTimers[0] <= TimeManager.Instance.RefTime) {
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
            if (_minigameTimers.Count > 0)
                return;

            float t = Random.Range(MIN_TIME, MAX_TIME);
            t = TimeManager.Instance.RefTime + t;

            if (!Setting.networked) {
                _refActor = Player.Local.ActorNumber;
                _minigameTimers.Add(t);
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
            for (int i = 0; i < _minigameTimers.Count; i++) {
                _minigameTimers[i] %= TimeManager.RESET_VALUE;
            }
        }

        private void OnTimerFin() {
            _minigameTimers.RemoveAt(0);
            Debug.LogError($"Timer fin actor {_refActor}");

            if (_refActor == Player.Local.ActorNumber) {
                ActivateMinigame();
            }

            if (_inActivity) {
                PushNewTimer();
            }
        }

        private void ResetMinigames() {
            if (Setting != null) {
                for (int i = 0; i < Setting.minigames.Length; i++) {
                    Setting.minigames[i].Reset();
                }
            }
        }

        public void OnActivityStart(int scoreMultiplier) {
            _inActivity = true;

            TimeManager.Instance.onRefTimeReset += OnTimeReset;

            if (Setting.networked) {
                Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
                Network.Local.Callbacks.onRoomPropertiesChanged += OnPropsChanged;
                Network.Local.Callbacks.onService += UpdateNetwork;

                if (Network.Local.Client.InRoom) {
                    OnJoinedRoom();
                }
            }

            _scoreMultiplier = scoreMultiplier;
            _interactables = FindObjectsOfType<MinigameInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();

            if (Setting.useTimer) {
                PushNewTimer();
            }
            else if (!AnyMinigameOpen()) {
                ActivateMinigame();
            }

            if (!Setting.networked) {
                onScoreReset?.Invoke();
            }
        }

        public void OnActivityStop() {
            if (_currentMinigame != null) {
                _currentMinigame.StopMinigame();
            }

            _inActivity = false;
            UILocator.Get<MinigameLogUI>().ClearLog();

            _minigameTimers.Clear();
            _refTime = -1;

            ResetMinigames();
        }

        private bool AnyMinigameOpen() {
            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].HasLearning) {
                    return true;
                }
            }

            return false;
        }

        private void ActivateMinigame() {
            if (TryGetNextMinigame(out MinigameDescription minigameDesc) &&
                TryGetInteractable(minigameDesc, out MinigameInteractable interactable)) {

                interactable.SetMinigame(minigameDesc);
            }
        }

        private bool TryGetNextMinigame(out MinigameDescription minigame) {
            int MinigameId;
            if (Setting.linear) {
                MinigameId = -1;

                for (int i = 0; i < Setting.minigames.Length; i++) {
                    if (Setting.minigames[i].MinigameState == MinigameDescription.State.Open) {
                        MinigameId = i;
                        break;
                    }
                }

                if (MinigameId == -1) {
                    onAllMinigamesFinished?.Invoke(HighscoreTracker.Instance.Local.score);
                    Debug.Log("All minigames finished");
                    
                    minigame = null;
                    return false;
                }
            }
            else {
                MinigameId = Random.Range(0, Setting.minigames.Length);
                bool found = false;
                int check = 0;
                for (int i = 0; i < Setting.minigames.Length; i++) {
                    check = (MinigameId + i) % Setting.minigames.Length;

                    if (Setting.minigames[check].MinigameState == MinigameDescription.State.Open) {
                        found = true;
                        MinigameId = check;
                        break;
                    }
                }

                if (!found) {
                    onAllMinigamesFinished?.Invoke(HighscoreTracker.Instance.Local.score);
                    Debug.Log("All minigames finished");
                    
                    minigame = null;
                    return false;
                }
            }

            minigame = Setting.minigames[MinigameId];
            return true;
        }

        private bool TryGetInteractable(MinigameDescription minigame, out MinigameInteractable interactable) {
            List<MinigameInteractable> options = new List<MinigameInteractable>();
            for (int iL = 0; iL < minigame.locations.Length; iL++) {
                for (int iI = 0; iI < _interactables.Length; iI++) {
                    if (_interactables[iI].Identifier == minigame.locations[iL]) {
                        options.Add(_interactables[iI]);
                        break;
                    }
                }
            }

            if (options.Count == 0) {
                interactable = null;
                return false;
            }

            int optionId = Random.Range(0, minigame.locations.Length);
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
                Debug.LogWarning($"No available interactable for learning {minigame.index}!");

                interactable = null;
                return false;
            }

            interactable = options[optionId];
            return true;
        }

        public void OnMinigameStart(MinigameDescription minigame, MinigameInteractable interactable) {
            InputManager.Instance.SetMinigameInput();
            _currenMinigameDescription = minigame;
            _currentInteractable = interactable;

            _currenMinigameDescription.SetState(MinigameDescription.State.Active, false);
            SceneManager.LoadScene(_currenMinigameDescription.sceneName, LoadSceneMode.Additive);
        }

        private void OnMinigameFinished(float scorePercentage) {
            _currentMinigame = null;
            InputManager.Instance.SetGameInput();
            MinigameDescription.State s = (scorePercentage > 0.001f)
                ? MinigameDescription.State.Completed
                : MinigameDescription.State.Failed;

            if (Setting.complete) {
                _currenMinigameDescription.SetState(s, false);
            }
            else {
                _currenMinigameDescription.SetState(MinigameDescription.State.Open, false, true);
            }

            if (Setting.networked) {
                PushMinigameState(_currenMinigameDescription);
            }

            onMinigameFinished?.Invoke(scorePercentage);

            _currenMinigameDescription.log.CheckLogItem(s);
            _currenMinigameDescription.log = null;

            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);

            //TODO_COHORT: fix the double call thingie?
            _currentInteractable.Deactivate();
            //_currentInteractable.SetMinigameLocal(-1);

            _currenMinigameDescription = null;
            _currentInteractable = null;

            if (!Setting.useTimer) {
                ActivateMinigame();
            }
        }

        private void PushMinigameState(MinigameDescription l) {
            Hashtable changes = new Hashtable();
            changes.Add(GetMinigameStateKey(l.index), l.MinigameState);

            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }

        private void SetMinigameState(int learningId, MinigameDescription.State state, bool init) {
            Setting.minigames[learningId].SetState(state, init);
        }

        public void SetMinigameLog(MinigameDescription minigame, MinigameInteractable interactable) {
            minigame.log = UILocator.Get<MinigameLogUI>()
                .CreateLogEntry(minigame.actionDescription, interactable.LocationDescription);
        }

        public void RemoveMinigameLog(MinigameDescription minigame) {
            if (minigame.log != null) {
                minigame.log.CheckLogItem(MinigameDescription.State.Failed);
                minigame.log = null;
            }
        }

        public void InitializeMinigame(Minigame minigame) {
            minigame.Initialize(_currenMinigameDescription.data, _scoreMultiplier, OnMinigameFinished);
            _currentMinigame = minigame;
        }

        private void OnJoinedRoom() {
            OnPropsChanged(Network.Local.Client.CurrentRoom.CustomProperties, true);

            string key = GetTimerKey();
            if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) ||
                (TimeManager.Instance.RefTime - (float)Network.Local.Client.CurrentRoom.CustomProperties[key]) >
                MAX_TIME) {
                InitTimerNetworkData();
            }
        }

        private void OnPropsChanged(Hashtable changes) {
            OnPropsChanged(changes, false);
        }

        private void OnPropsChanged(Hashtable changes, bool init) {
            string key;
            for (int i = 0; i < Setting.minigames.Length; i++) {
                key = GetMinigameStateKey(i);

                if (changes.ContainsKey(key)) {
                    MinigameDescription.State s;
                    if (changes[key] == null) {
                        s = MinigameDescription.State.Open;
                    }
                    else {
                        s = (MinigameDescription.State)changes[key];
                    }

                    SetMinigameState(i, s, init);
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
                                _minigameTimers.Clear();
                        }
                        else {
                            _minigameTimers.Add(_refTime);
                        }
                    }
                    else {
                        InitTimerNetworkData();
                    }
                }
            }
        }

        private string GetMinigameStateKey(int learningId) {
            return Keys.Concatenate(
                Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.State),
                learningId.ToString());
        }

        private string GetTimerKey() {
            return Keys.Concatenate(
                Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.ManagerTimer));
        }

        private string GetActorKey() {
            return Keys.Concatenate(
                Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.ManagerActor));
        }
    }
}