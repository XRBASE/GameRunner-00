using Cohort.Networking.PhotonKeys;
using Cohort.CustomAttributes;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.GameRunner.Interaction {
    public abstract class Interactable : MonoBehaviour, IUniqueId {
        protected const int LAYER = 6;

        public bool Active {
            get { return _active; }
        }

        public int Identifier {
            get { return _index; }
            set { _index = value; }
        }

        [ReadOnly, SerializeField] private int _index;

        //all interactables always have a state. True will fire events, false will not.
        //event like interactions will fire and directly reset themselves, whereas more
        //permanent Interactables retain it.

        [SerializeField] private bool _active = false;
        [SerializeField] private bool _networked = true;
        private bool _initial = true;
        
        protected virtual void Start() {
            gameObject.layer = LAYER;
            
            if (_networked) {
                Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
                Network.Local.Callbacks.onRoomPropertiesChanged += OnPropertiesChanged;

                if (Network.Local.Client.InRoom) {
                    OnJoinedRoom();
                }
            }
        }

        public abstract void OnInteract();

        public virtual void Activate() {
            Activate(null, null);
        }

        public virtual void Deactivate() {
            Deactivate(null, null);
        }

        protected virtual void Activate(Hashtable changes, Hashtable expected = null) {
            Debug.LogError($"Activate");
            if (!_networked) {
                OnActivate();
                return;
            }

            if (changes == null) {
                changes = new Hashtable();
            }

            changes.Add(GetInteractableKey(), true);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
        }

        protected virtual void Deactivate(Hashtable changes, Hashtable expected = null) {
            Debug.LogError($"Deactivate");
            if (!_networked) {
                OnDeactivate();
                return;
            }

            if (changes == null) {
                changes = new Hashtable();
            }

            changes.Add(GetInteractableKey(), false);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
        }

        private void ChangeState(bool newState, bool force = false) {
            if (!(force || _initial) && _active == newState)
                return;
            
            _initial = false;
            _active = newState;
            if (_active) {
                OnActivate();
            }
            else {
                OnDeactivate();
            }
        }

        protected abstract void OnActivate();

        protected abstract void OnDeactivate();

        protected virtual void OnJoinedRoom() {
            OnPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);
            if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(GetInteractableKey())) {
                Deactivate();
            }
        }

        protected virtual void OnPropertiesChanged(Hashtable changes) {
            string key = GetInteractableKey();
            if (changes.ContainsKey(key)) {
                if (changes[key] == null) {
                    Deactivate();
                }
                else {
                    ChangeState((bool)changes[key]);
                }
            }
        }

        protected string GetInteractableKey() {
            return Keys.GetUUID(Keys.Room.Interactable, _index.ToString());
        }
    }
}