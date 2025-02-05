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
    [DefaultExecutionOrder(1)] //After LearningInteractable
    public class MinigameManager : Singleton<MinigameManager> {
        public const float FAILURE_THRESHOLD = 0.25f;
        public const float AMAZING_THRESHOLD = 0.75f;

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

        public Action<int> onAllMinigamesFinished;
        public Action<Minigame.FinishCause, float> onMinigameFinished;

        private int _scoreMultiplier;
        private MinigameInteractable[] _interactables;

        //this is the currently open interactable and learning for the local user. There should never be two minigames open at the same time.
        private MinigameDescription _currenMinigameDescription;
        private Minigame _currentMinigame;
        private MinigameInteractable _currentInteractable;

        private void Start() {
            //join room is called when the activity is started
            Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropertiesChanged;
        }

        private void OnDestroy() {
            Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoomPropertiesChanged;
        }
        
        private void OnJoinedRoom() {
            OnRoomPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties, true);
        }

        private void OnRoomPropertiesChanged(Hashtable changes) {
            OnRoomPropertiesChanged(changes, false);
        }

        private void OnRoomPropertiesChanged(Hashtable changes, bool initial) {
            string key = GetMinigamePlayerKey();
            
            int minigameIndex = -1;
            MinigameDescription.State newState;
            foreach (var kv_change in changes) {
                if (kv_change.Key.ToString().StartsWith(key)) {
                    minigameIndex = GetMinigameIndexFromKey(kv_change.Key.ToString());
                    
                    OnMinigameStateChanged(Setting.minigames[minigameIndex], (string)kv_change.Value, initial);
                }
            }
        }
        
        private void SetMinigameState(int index, MinigameDescription.Status status, int location) {
            MinigameDescription.State state = new MinigameDescription.State();
            state.location = location;
            state.status = status;
            
            Hashtable changes = new Hashtable();
            changes.Add(GetMinigamePlayerKey(index), JsonUtility.ToJson(state));

            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }

        private void OnMinigameStateChanged(MinigameDescription minigame, string data, bool initial) {
            if (string.IsNullOrEmpty(data)){
                Debug.LogWarning("Minigame null, data reset!");
                
                minigame.Reset();
                return;
            }

            MinigameDescription.State newState = JsonUtility.FromJson<MinigameDescription.State>(data);
            minigame.SetState(newState, initial);

            for (int i = 0; i < _interactables.Length; i++) {
                if (_interactables[i].Identifier == newState.location && !_interactables[i].HasMinigame) {
                    _interactables[i].SetMinigame(minigame.index, minigame.networked);
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

        public void OnActivityStart(int scoreMultiplier) {
            _scoreMultiplier = scoreMultiplier;
            _interactables = FindObjectsOfType<MinigameInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();

            if (Network.Local.Client.InRoom) {
                OnJoinedRoom();
            }
            
            if (!AnyMinigameOpen()) {
                ActivateMinigame();
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
                
                SetMinigameState(minigameDesc.index, MinigameDescription.Status.Available, interactable.Identifier);
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
            
            SetMinigameState(_currenMinigameDescription.index, MinigameDescription.Status.Open, _currenMinigameDescription.state.location);
            
            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);
            
            _currentInteractable.Deactivate();
            
            _currenMinigameDescription = null;
            _currentInteractable = null;
        }
        
        private void OnMinigameFinished(Minigame.FinishCause cause, float scorePercentage) {
            _currentMinigame = null;
            InputManager.Instance.SetGameInput();
            MinigameDescription.Status s = (scorePercentage > 0.001f)
                ? MinigameDescription.Status.Completed
                : MinigameDescription.Status.Failed;
            
            SetMinigameState(_currenMinigameDescription.index, s, -1);
            
            onMinigameFinished?.Invoke(cause, scorePercentage);

            _currenMinigameDescription.log.CheckLogItem(s);
            _currenMinigameDescription.log = null;

            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);
            
            _currentInteractable.Deactivate();
            _currentInteractable.SetMinigame(-1, false);

            _currenMinigameDescription = null;
            _currentInteractable = null;
            ActivateMinigame();
        }

        public void SetMinigameLog(MinigameDescription minigame, MinigameInteractable interactable) {
            minigame.log = UILocator.Get<MinigameLogUI>()
                .CreateLogEntry(minigame.actionDescription, interactable.LocationDescription);
        }

        public void InitializeMinigame(Minigame minigame) {
            minigame.Initialize(_currenMinigameDescription.data, _currenMinigameDescription.timeLimit, OnMinigameFinished, OnExitMinigame);
            _currentMinigame = minigame;
        }

        private string GetMinigamePlayerKey(int minigameIndex = -1) {
            if (minigameIndex >= 0) {
                return Keys.Concatenate(Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.State),
                                        Player.Local.UUID, minigameIndex.ToString());
            }
            else {
                return Keys.Concatenate(Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.State),
                                        Player.Local.UUID);
            }
        }

        private int GetMinigameIndexFromKey(string key) {
            //returns second to last part of key, split by key separator and cast to an int.
            return int.Parse(key.Split(Keys.SEPARATOR)[^1]);
        }
    }
}