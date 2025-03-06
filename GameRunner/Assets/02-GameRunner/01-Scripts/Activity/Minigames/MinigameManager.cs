using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Cohort.GameRunner.Input;
using Cohort.GameRunner.Score;
using Cohort.Networking.PhotonKeys;
using Cohort.Patterns;
using Cohort.UI.Generic;
using ExitGames.Client.Photon;
using Player = Cohort.GameRunner.Players.Player;
using Random = UnityEngine.Random;

namespace Cohort.GameRunner.Minigames {
    [DefaultExecutionOrder(102)] //After MinigameInteractable
    public class MinigameManager : Singleton<MinigameManager> {
        //maybe make this a part of the minigame flow or finish cause or something
        public const float FAILURE_THRESHOLD = 0.25f;
        public const float AMAZING_THRESHOLD = 0.75f;

        public MinigameCycleDescription Setting {
            get { return _setting;}
            set {
                _setting = value;
                if (!_initialized) {
                    OnInitialize();
                }
            }
        }
        
        private MinigameCycleDescription _setting;

        public MinigameDescription this[int index] {
            get { return Setting.minigames[index]; }
        }

        public MinigameDescription Current {
            get { return _currenMinigameDescription; }
        }

        public Action<int> onAllMinigamesFinished;
        public Action<Minigame.FinishCause, int> onMinigameFinished;
        
        private MinigameInteractable[] _interactables;

        //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
        private MinigameDescription _currenMinigameDescription;
        private Minigame _currentMinigame;
        private MinigameInteractable _currentInteractable;
        private bool _initialized;

        private void Start() {
            //join room is called when the activity is started
            Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
        }

        private void OnDestroy() {
            Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoomPropertiesChanged;
            _initialized = false;
        }
        
        private void OnJoinedRoom() {
            OnRoomPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties, true);
        }

        private void OnRoomPropertiesChanged(Hashtable changes) {
            OnRoomPropertiesChanged(changes, false);
        }

        private void OnRoomPropertiesChanged(Hashtable changes, bool initial) {
            if (!_initialized)
                return;
            
            string key = GetMinigamePlayerKey();
            
            int minigameIndex = -1;
            MinigameDescription.State[] nStates = new MinigameDescription.State[Setting.minigames.Count];
            bool hasUuid;
            string data;
            string uuid;

            foreach (var kv_change in changes) {
                if (!kv_change.Key.ToString().StartsWith(key))
                    continue;
                
                hasUuid = kv_change.Key.ToString().Split(Keys.SEPARATOR)[^1].Length > 2;
                minigameIndex = GetMinigameIndexFromKey(kv_change.Key.ToString(), hasUuid);
                uuid = GetUUIDFromKey(kv_change.Key.ToString());
                data = (string)kv_change.Value;

                if (hasUuid && uuid != Player.Local.UUID) {
                    continue;
                }
                    
                if (!string.IsNullOrEmpty(data)) {
                    //override data only if there is a uuid
                    if (nStates[minigameIndex] == null || hasUuid) {
                        nStates[minigameIndex] = JsonUtility.FromJson<MinigameDescription.State>(data);
                        
                        //only override status if it is done locally, or the status is changed into a higher form networked.
                        if (!hasUuid && nStates[minigameIndex].status <= _setting.minigames[minigameIndex].state.status) {
                            nStates[minigameIndex] = null;
                        }
                    }
                        
                }
                else if (hasUuid) {
                    //todo: reset
                    Setting.minigames[minigameIndex].Reset();
                    nStates[minigameIndex] = null;
                }
            }

            for (int i = 0; i < nStates.Length; i++) {
                if (nStates[i] != null) {
                    OnMinigameStateChanged(Setting.minigames[i], nStates[i], initial);
                }
            }
        }

        private Hashtable GetMinigameStateChangeTable(int index, MinigameDescription.Status status, int location, bool networked, Hashtable changes = null) {
            if (changes == null) {
                changes = new Hashtable();
            }
            
            MinigameDescription.State state = new MinigameDescription.State();
            state.location = location;
            state.status = status;
            
            if (!networked) {
                changes.Add(GetMinigamePlayerKey(index, Player.Local.UUID), JsonUtility.ToJson(state));
            }
            else {
                changes.Add(GetMinigamePlayerKey(index), JsonUtility.ToJson(state));
            }

            return changes;
        }

        private void OnMinigameStateChanged(MinigameDescription minigame, MinigameDescription.State state, bool initial) {
            minigame.SetState(state, initial);
            
            if (state.status == MinigameDescription.Status.Active) {
                OnMinigameStart(minigame);
                return;
            }

            if (state.status == MinigameDescription.Status.Available) {
                for (int i = 0; i < _interactables.Length; i++) {
                    if (_interactables[i].Identifier == state.location && !_interactables[i].HasMinigame) {
                        _interactables[i].SetMinigame(minigame.index);
                    }
                }
            }
        }

        private void ResetMinigames() {
            if (Setting != null) {
                for (int i = 0; i < Setting.minigames.Count; i++) {
                    Setting.minigames[i].Reset();
                }
            }

            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].HasMinigame) {
                    _interactables[i].SetMinigame(-1);
                }
            }
        }

        public void OnActivityStart() {
            _interactables = FindObjectsOfType<MinigameInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();

            OnInitialize();
        }

        private void OnInitialize() {
            if (Setting != null && _interactables != null) {
                _initialized = true;
                
                if (Network.Local.Client.InRoom) {
                    OnJoinedRoom();
                }
            
                if (!AnyMinigameOpen()) {
                    ActivateMinigames();
                }
            }
        }

        public void OnActivityStop() {
            if (_currentMinigame != null) {
                _currentMinigame.FinishMinigame(Minigame.FinishCause.ActivityStop);
            }

            UILocator.Get<MinigameLogUI>().ClearLog();
            ResetMinigames();
        }

        private bool AnyMinigameOpen() {
            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].HasMinigame) {
                    return true;
                }
            }

            return false;
        }


        private bool AllGamesFinished() {
            int prevIndex = (_currenMinigameDescription == null) ? -1 : _currenMinigameDescription.index;
            for (int i = 0; i < Setting.minigames.Count; i++) {
                if (Setting.minigames[i].required && Setting.minigames[i].state.status <= MinigameDescription.Status.Active && i != prevIndex) {
                    Debug.LogError($"Minigame {i} not done, status is {Setting.minigames[i].state.status}.");
                    return false;
                }
            }
            
            return true;
        }
        
        private void ActivateMinigames() {
            if (!TryGetPhaseIndex(out int phase)) {
                if (AllGamesFinished()) {
                    onAllMinigamesFinished?.Invoke(HighscoreTracker.Instance.Local.score);
                
                    Debug.LogWarning("All minigames finished");
                }
                return;
            }
            //current is set to null here, so the all games finished can account for the previously finished minigame.
            _currenMinigameDescription = null;
            
            bool hasChanges = false;
            List<int> takenInteractables = new List<int>();
            Hashtable changes = new Hashtable();

            for (int i = 0; i < Setting.minigames.Count; i++) {
                if (Setting.minigames[i].phase != phase)
                    continue;
                
                if (Setting.minigames[i].state.status == MinigameDescription.Status.Open) {
                    if (TryGetInteractable(Setting.minigames[i], out MinigameInteractable interactable) && !takenInteractables.Contains(interactable.Identifier))
                    {
                        changes = GetMinigameStateChangeTable(Setting.minigames[i].index, MinigameDescription.Status.Available, interactable.Identifier,
                                                           Setting.minigames[i].networked, changes);
                        
                        takenInteractables.Add(interactable.Identifier);
                        hasChanges = true;
                    }
                    else {
                        Debug.LogWarning($"No available interactable for learning {Setting.minigames[i].index}!");
                    }
                }
            }

            if (hasChanges) {
                Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
            }
        }

        public void StartMinigameById(int index) {
            if (Setting.minigames[index].state.status == MinigameDescription.Status.Open) {
                StartMinigame(Setting.minigames[index], null);
            }
            else {
                Debug.LogWarning($"Minigame {index} already finished!");
            }
        }

        private bool TryGetPhaseIndex(out int phase) {
            phase = int.MaxValue;
            bool found = false; 
            
            for (int i = 0; i < Setting.minigames.Count; i++) {
                if (Setting.minigames[i].state.status is MinigameDescription.Status.Open or MinigameDescription.Status.Available &&
                    Setting.minigames[i].phase < phase && Setting.minigames[i].phase >= 0) {
                    phase = Setting.minigames[i].phase;
                    
                    found = true;
                }
            }

            return found;
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
                if (!options[check].HasMinigame) {
                    optionId = check;
                    found = true;
                    break;
                }
            }

            if (!found) {
                Debug.LogWarning($"No available interactable for minigame {minigame.index}!");

                interactable = null;
                return false;
            }

            interactable = options[optionId];
            return true;
        }

        public void StartMinigame(MinigameDescription minigame, MinigameInteractable interactable) {
            Hashtable changes = GetMinigameStateChangeTable(minigame.index,
                                                            MinigameDescription.Status.Active,
                                                            minigame.state.location, minigame.networked);
            
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }

        public void OnMinigameStart(MinigameDescription minigame) {
            if (minigame.state.location >= 0) {
                for (int i = 0; i < _interactables.Length; i++) {
                    if (_interactables[i].Identifier == minigame.state.location && _interactables[i].MinigameIndex == minigame.index) {
                        _currentInteractable = _interactables[i];
                    }
                }
            }
            
            if (_currentMinigame != null) {
                _currentMinigame.ExitMinigame();
            }
            _currenMinigameDescription = minigame;
            
            InputManager.Instance.SetMinigameInput();
            SceneManager.LoadScene(_currenMinigameDescription.sceneName, LoadSceneMode.Additive);
        }

        private void OnExitMinigame() {
            InputManager.Instance.SetGameInput();

            Hashtable changes = GetMinigameStateChangeTable(_currenMinigameDescription.index,
                                                            MinigameDescription.Status.Open,
                                                            _currenMinigameDescription.state.location, false);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
            
            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);

            if (_currentInteractable) {
                _currentInteractable.Deactivate();
            }
            
            _currenMinigameDescription = null;
            _currentInteractable = null;
        }
        
        private void OnMinigameFinished(Minigame.FinishCause cause, int score) {
            _currentMinigame = null;
            InputManager.Instance.SetGameInput();
            
            MinigameDescription.Status s;
            switch (cause) {
                case Minigame.FinishCause.FinSuccess:
                case Minigame.FinishCause.FinPerfect:
                    s = MinigameDescription.Status.FinSuccess;
                    break;
                case Minigame.FinishCause.FinFailed:
                case Minigame.FinishCause.Timeout:
                case Minigame.FinishCause.ActivityStop: 
                default:
                    s = MinigameDescription.Status.FinFailed;
                    break;
            }

            Hashtable changes = GetMinigameStateChangeTable(_currenMinigameDescription.index, s, -1, false);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
            
            onMinigameFinished?.Invoke(cause, score);

            if (_currenMinigameDescription.log) {
                _currenMinigameDescription.log.CheckLogItem(s);
                _currenMinigameDescription.log = null;
            }
            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);

            if (_currentInteractable) {
                _currentInteractable.Deactivate();
                _currentInteractable.SetMinigame();
            }
            
            _currentInteractable = null;
            ActivateMinigames();
        }

        public void SetMinigameLog(MinigameDescription minigame, MinigameInteractable interactable) {
            minigame.log = UILocator.Get<MinigameLogUI>()
                .CreateLogEntry(minigame.actionDescription, interactable.LocationDescription);
        }

        public void InitializeMinigame(Minigame minigame) {
            minigame.Initialize(_currenMinigameDescription.data, _currenMinigameDescription.timeLimit,
                                _currenMinigameDescription.minScore, _currenMinigameDescription.maxScore,
                                OnMinigameFinished, OnExitMinigame);
            
            _currentMinigame = minigame;
        }

        private string GetMinigamePlayerKey(int minigameIndex = -1, string uuid = "") {
            string postfix = "";
            if (minigameIndex >= 0) {
                if (!string.IsNullOrEmpty(uuid)) {
                    postfix = Keys.Concatenate(minigameIndex.ToString(), uuid);
                }
                else {
                    postfix = minigameIndex.ToString();
                }
                
                return Keys.Concatenate(Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.State),
                                        postfix);
            }
            else {
                return Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.State);
            }
        }

        private int GetMinigameIndexFromKey(string key, bool hasUuid) {
            //returns second to last part of key, split by key separator and cast to an int.
            if (!hasUuid) {
                return int.Parse(key.Split(Keys.SEPARATOR)[^1]);
            }
            else {
                return int.Parse(key.Split(Keys.SEPARATOR)[^2]);
            }
        }
        
        private string GetUUIDFromKey(string key) {
            //returns last part of key, split by key separator.
            return key.Split(Keys.SEPARATOR)[^1];
        }
    }
}