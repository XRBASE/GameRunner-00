using ExitGames.Client.Photon;
using UnityEngine;

using Cohort.CustomAttributes;
using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;

namespace Cohort.GameRunner.Minigames {
    [DefaultExecutionOrder(0)] //Before learningManager
    public class MinigameInteractable : Interactable {

        public string LocationDescription {
            get { return _locationDescription; }
        }

        public bool HasLearning {
            get { return _hasLearning; }
            private set {
                interactable = value;
                _hasLearning = value;
            }
        }

        [ReadOnly, SerializeField] private bool _hasLearning;

        [SerializeField] private string _locationDescription = "At position";
        [SerializeField] private ObjIndicator _indicator;

        private int _actor = -1;
        private MinigameDescription _minigame;

        private void Awake() {
            interactable = false;
        }

        protected override void Start() {
            base.Start();

            if (_networked) {
                Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
            }
        }

        public override void SetInRange(bool value) {
            base.SetInRange(value);

            _indicator.SetActive(!InRange && HasLearning);
        }

        public override void OnInteract() {
            if (!HasLearning) {
                return;
            }

            Activate();
        }

        protected override void Activate(Hashtable changes, Hashtable expected = null) {
            if (!_networked) {
                base.Activate(null, null);
                return;
            }

            if (changes == null)
                changes = new Hashtable();
            if (expected == null)
                expected = new Hashtable();

            changes.Add(GetActorKey(), Player.Local.ActorNumber);
            expected.Add(GetInteractableKey(), false);

            base.Activate(changes, expected);
        }

        protected override void ActivateLocal() {
            _indicator.SetActive(false);

            if (!_networked || _actor == Player.Local.ActorNumber) {
                MinigameManager.Instance.OnMinigameStart(_minigame, this);
            }
        }

        protected override void Deactivate(Hashtable changes, Hashtable expected = null) {
            if (!_networked) {
                base.Deactivate(changes, expected);
                return;
            }

            if (changes == null)
                changes = new Hashtable();

            changes.Add(GetActorKey(), null);

            //if there is an actor this item can only be deactivated by that actor.
            if (_actor != -1) {
                if (expected == null)
                    expected = new Hashtable();

                expected.Add(GetActorKey(), Player.Local.ActorNumber);
            }

            base.Deactivate(changes, expected);
        }

        protected override void DeactivateLocal() {
            //SetMinigameLocal(-1);
        }

        public void SetMinigame(int index = -1) {
            if (!_networked) {
                SetMinigameLocal(index);
                return;
            }

            Hashtable changes = new Hashtable();
            changes.Add(GetMinigameKey(), index);

            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }

        public void SetMinigameLocal(int index) {
            HasLearning = index >= 0;

            if (HasLearning) {
                _minigame = MinigameManager.Instance[index];

                _indicator.SetActive(!InRange && HasLearning);
                MinigameManager.Instance.SetMinigameLog(_minigame, this);
                return;
            }

            _indicator.SetActive(false);

            if (_minigame != null) {
                MinigameManager.Instance.RemoveMinigameLog(_minigame);
                _minigame = null;
            }
        }

        private void OnPlayerLeftRoom(Photon.Realtime.Player obj) {
            if (_actor == obj.ActorNumber) {
                _actor = -1;
                Deactivate();
            }
        }

        protected override void OnJoinedRoom() {
            base.OnJoinedRoom();

            if (Value && !PlayerManager.Instance.ActorNumberExists(_actor)) {
                _actor = -1;
                Deactivate();
            }
        }

        protected override void OnPropertiesChanged(Hashtable changes) {
            string key = GetMinigameKey();
            if (changes.ContainsKey(key)) {
                if (changes[key] == null) {
                    SetMinigameLocal(-1);
                }
                else {
                    SetMinigameLocal((int)changes[key]);
                }
            }

            key = GetActorKey();
            if (changes.ContainsKey(key)) {
                if (changes[key] == null) {
                    _actor = -1;
                }
                else {
                    _actor = (int)changes[key];
                }
            }

            base.OnPropertiesChanged(changes);
        }

        private string GetActorKey() {
            return Keys.Concatenate(
                Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.Actor),
                Identifier.ToString());
        }

        private string GetMinigameKey() {
            return Keys.Concatenate(
                Keys.Concatenate(Keys.Room.Minigame, Keys.Minigame.Index),
                Identifier.ToString());
        }
    }
}