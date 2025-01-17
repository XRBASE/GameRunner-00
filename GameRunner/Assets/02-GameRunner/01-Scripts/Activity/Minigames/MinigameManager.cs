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

        private void ResetMinigames() {
            if (Setting != null) {
                for (int i = 0; i < Setting.minigames.Length; i++) {
                    Setting.minigames[i].Reset();
                }
            }
        }

        public void OnActivityStart(int scoreMultiplier) {
            _inActivity = true;

            _scoreMultiplier = scoreMultiplier;
            _interactables = FindObjectsOfType<MinigameInteractable>().ToList().OrderBy((s) => s.Identifier).ToArray();

            if (!AnyMinigameOpen()) {
                ActivateMinigame();
            }
            onScoreReset?.Invoke();
        }

        public void OnActivityStop() {
            if (_currentMinigame != null) {
                _currentMinigame.FinishMinigame();
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

                interactable.SetMinigame(minigameDesc.index);
            }
        }

        private bool TryGetNextMinigame(out MinigameDescription minigame) {
            int MinigameId;
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

        private void OnExitMinigame() {
            InputManager.Instance.SetGameInput();
            
            _currenMinigameDescription.SetState(MinigameDescription.State.Open, false);
            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);
            
            _currentInteractable.Deactivate();
            
            _currenMinigameDescription = null;
            _currentInteractable = null;
            
            //throw new NotImplementedException("Mwaap");
        }
        
        private void OnMinigameFinished(float scorePercentage) {
            _currentMinigame = null;
            InputManager.Instance.SetGameInput();
            MinigameDescription.State s = (scorePercentage > 0.001f)
                ? MinigameDescription.State.Completed
                : MinigameDescription.State.Failed;

            _currenMinigameDescription.SetState(s, false);
            
            onMinigameFinished?.Invoke(scorePercentage);

            _currenMinigameDescription.log.CheckLogItem(s);
            _currenMinigameDescription.log = null;

            SceneManager.UnloadSceneAsync(_currenMinigameDescription.sceneName);

            //TODO_COHORT: this doesn't work yet when the game is networked, as the interactable is not cleared instantly
            _currentInteractable.Deactivate();
            _currentInteractable.SetMinigame(-1);

            _currenMinigameDescription = null;
            _currentInteractable = null;
            ActivateMinigame();
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
            minigame.Initialize(_currenMinigameDescription.data, _scoreMultiplier, OnMinigameFinished, OnExitMinigame);
            _currentMinigame = minigame;
        }
    }
}