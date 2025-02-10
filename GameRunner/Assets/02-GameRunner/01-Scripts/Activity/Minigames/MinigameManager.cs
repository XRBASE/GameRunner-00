using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

using Cohort.GameRunner.Input;
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
            MinigameDescription.State[] nStates = new MinigameDescription.State[Setting.minigames.Length];
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
                    }
                        
                }
                else if (hasUuid) {
                    //todo: reset
                    Setting.minigames[minigameIndex].Reset();
                    nStates[minigameIndex] = Setting.minigames[minigameIndex].state;
                }
            }

            for (int i = 0; i < nStates.Length; i++) {
                if (nStates[i] != null) {
                    OnMinigameStateChanged(Setting.minigames[i], nStates[i], initial);
                }
            }
        }
        
        private void SetMinigameState(int index, MinigameDescription.Status status, int location, bool networked) {
            MinigameDescription.State state = new MinigameDescription.State();
            state.location = location;
            state.status = status;
            
            Hashtable changes = new Hashtable();
            if (!networked) {
                changes.Add(GetMinigamePlayerKey(index, Player.Local.UUID), JsonUtility.ToJson(state));
            }
            else {
                changes.Add(GetMinigamePlayerKey(index), JsonUtility.ToJson(state));
            }
            //okay so now we network the message to everyone, but when I stop the game, it should only be networked for me. 

            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }

        private void OnMinigameStateChanged(MinigameDescription minigame, MinigameDescription.State state, bool initial) {
            //only override status if it is done locally, or the status is changed into a higher form networked.
            minigame.SetState(state, initial);
            
            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].Identifier == state.location && !_interactables[i].HasMinigame) {
                    _interactables[i].SetMinigame(minigame.index);
                }
            }
        }

        private void ResetMinigames() {
            if (Setting != null) {
                for (int i = 0; i < Setting.minigames.Length; i++) {
                    Setting.minigames[i].Reset();
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
                    ActivateMinigame();
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

        private void ActivateMinigame() {
            if (TryGetNextMinigame(out MinigameDescription minigameDesc) &&
                TryGetInteractable(minigameDesc, out MinigameInteractable interactable)) {
                
                SetMinigameState(minigameDesc.index, MinigameDescription.Status.Available, interactable.Identifier, minigameDesc.networked);
            }
        }

        private bool TryGetNextMinigame(out MinigameDescription minigame) {
            int MinigameId;
            MinigameId = -1;

            for (int i = 0; i < Setting.minigames.Length; i++) {
                //TODO: minigames that are available should be assigned based on their network data, rather than just opening them.
                
                if (Setting.minigames[i].state.status is MinigameDescription.Status.Open or MinigameDescription.Status.Available) {
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
                if (!options[check].HasMinigame) {
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
            if (_currentMinigame != null) {
                _currentMinigame.ExitMinigame();
            }
            
            InputManager.Instance.SetMinigameInput();
            _currenMinigameDescription = minigame;
            _currentInteractable = interactable;
            
            _currenMinigameDescription.SetStatus(MinigameDescription.Status.Active, false);
            //SetMinigameState(_currenMinigameDescription.index, MinigameDescription.State.Active);
            
            SceneManager.LoadScene(_currenMinigameDescription.sceneName, LoadSceneMode.Additive);
        }

        private void OnExitMinigame() {
            InputManager.Instance.SetGameInput();
            
            SetMinigameState(_currenMinigameDescription.index, MinigameDescription.Status.Open, _currenMinigameDescription.state.location, false);
            
            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);
            
            _currentInteractable.Deactivate();
            
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
            
            SetMinigameState(_currenMinigameDescription.index, s, -1, false);
            
            onMinigameFinished?.Invoke(cause, score);

            _currenMinigameDescription.log.CheckLogItem(s);
            _currenMinigameDescription.log = null;

            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);
            
            _currentInteractable.Deactivate();
            _currentInteractable.SetMinigame();

            _currenMinigameDescription = null;
            _currentInteractable = null;
            ActivateMinigame();
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