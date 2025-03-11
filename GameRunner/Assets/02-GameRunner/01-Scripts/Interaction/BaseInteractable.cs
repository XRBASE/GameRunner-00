using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.GameRunner.Interaction {
    public abstract class BaseInteractable : UniqueId {
        protected const int LAYER = 6;

        public bool Value {
            get { return _value; }
        }

        public override string Name {
            get { return gameObject.name; }
        }
        
        public bool InInteractRange { get; protected set; }
        protected bool InViewRange { get; set; }

        protected bool Initial {
            get { return (_networked && _initial); }
        }

        public bool interactable = true;
        
        [Tooltip("Can only be activated within this radius"), SerializeField] private float _interactRadius = 1;
        
        [SerializeField] protected ObjIndicator _indicator;
        [Tooltip("Icon is shown within this radius, negative value will always be visible"), SerializeField] 
        protected float _viewRadius = -1;
        
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
        protected virtual void Update() {
            CheckInteractRange();
            CheckViewRange();

            if (_indicator != null) {
                _indicator.SetActive(IndicatorActive());
            }
        }

        protected virtual bool IndicatorActive() {
            return InViewRange && !InInteractRange;
        }

        public virtual bool CheckViewRange() {
            if (_viewRadius < 0)
                InViewRange = true;
            else {
                InViewRange = (transform.position - Player.Local.transform.position).magnitude <= _viewRadius;
            }

            return InViewRange;
        }

        protected virtual bool CheckInteractRange() {
            InInteractRange = (transform.position - Player.Local.transform.position).magnitude <= _interactRadius;
            return InInteractRange;
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
            
            _value = newState;
            if (_value) {
                ActivateLocal();
            }
            else {
                DeactivateLocal();
            }
            
            _initial = false;
        }

        protected virtual void ActivateLocal() {
            _value = true;
        }

        protected virtual void DeactivateLocal() {
            _value = false;
        }

        protected virtual void OnJoinedRoom() {
            if (!_networked)
                return;
            
            OnPropertiesChanged(Network.Local.Client.CurrentRoom.CustomProperties);
            
            if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(GetInteractableKey())) {
                Deactivate();
            }
        }

        protected virtual void OnPropertiesChanged(Hashtable changes) {
            if (!_networked)
                return;
            
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

        protected virtual string GetInteractableKey() {
            return Keys.GetUUID(Keys.Room.Interactable, Identifier.ToString());
        }

#if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected() {
            Gizmos.DrawWireSphere(transform.position, _interactRadius);
        }
#endif
    }
}