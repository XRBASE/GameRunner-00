using Cohort.CustomAttributes;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.GameRunner.Interaction {
    public abstract class Interactable : MonoBehaviour, IUniqueId {
        protected const int LAYER = 6;

        public bool Value {
            get { return _value; }
        }

        public int Identifier {
            get { return _index; }
            set { _index = value; }
        }

        [ReadOnly, SerializeField] private int _index = -1;

        //all interactables always have a state. True will fire events, false will not.
        //event like interactions will fire and directly reset themselves, whereas more
        //permanent Interactables retain it.

        [Tooltip("Current state on/off"), SerializeField] private bool _value = false;
        [SerializeField] protected bool _networked = true;
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

        protected virtual void OnDestroy() {
            if (_networked) {
                Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
                Network.Local.Callbacks.onRoomPropertiesChanged -= OnPropertiesChanged;
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
            if (!_networked) {
                ActivateLocal();
                return;
            }

            if (changes == null) {
                changes = new Hashtable();
            }

            changes.Add(GetInteractableKey(), true);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
        }

        protected virtual void Deactivate(Hashtable changes, Hashtable expected = null) {
            if (!_networked) {
                DeactivateLocal();
                return;
            }

            if (changes == null) {
                changes = new Hashtable();
            }

            changes.Add(GetInteractableKey(), false);
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
        }

        private void ChangeState(bool newState, bool force = false) {
            if (!(force || _initial) && _value == newState)
                return;
            
            _initial = false;
            _value = newState;
            if (_value) {
                ActivateLocal();
            }
            else {
                DeactivateLocal();
            }
        }

        protected abstract void ActivateLocal();

        protected abstract void DeactivateLocal();

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