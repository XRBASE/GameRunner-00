using ExitGames.Client.Photon;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Cohort.CustomAttributes;
using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;

namespace Cohort.GameRunner.Minigames {
    public class MinigameInteractable : BaseInteractable {

        public string LocationDescription {
            get { return _locationDescription; }
        }

        public bool HasMinigame {
            get { return hasMinigame; }
            private set {
                interactable = value;
                hasMinigame = value;
            }
        }

        public int MinigameIndex {
            get { return HasMinigame? _minigame.index : -1; }
        }
        
        protected bool InViewRange { get; private set; }

        [ReadOnly, SerializeField] private bool hasMinigame;

        [SerializeField] private string _locationDescription = "At position";
        [SerializeField] private ObjIndicator _indicator;
        
        [Tooltip("Icon is shown within this radius, negative value will always be visible"), SerializeField] 
        private float _viewRadius = -1;

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

        protected override void OnDestroy() {
            base.OnDestroy();
            
            if (_networked) {
                Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
            }

            if (HasMinigame && _minigame.log) {
                _minigame.log.RemoveDirect();
            }
        }

        protected override void Update() {
            base.Update();
            CheckViewRange();

            _indicator.SetActive( InViewRange && !InInteractRange && HasMinigame);
        }

        public virtual bool CheckViewRange() {
            if (_viewRadius < 0)
                InViewRange = true;
            else {
                InViewRange = (transform.position - Player.Local.transform.position).magnitude <= _viewRadius;
            }

            return InViewRange;
        }

        public override void OnInteract() {
            if (!HasMinigame) {
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
            MinigameManager.Instance.StartMinigame(_minigame, this);
        }

        protected override void Deactivate(Hashtable changes, Hashtable expected = null) {
            if (!_networked) {
                base.Deactivate(changes, expected);
                return;
            }

            if (changes == null)
                changes = new Hashtable();

            changes.Add(GetActorKey(), null);
            
            //Should we even track actor on deactivate?
            
            //if there is an actor this item can only be deactivated by that actor.
            if (_actor != -1) {
                if (expected == null)
                    expected = new Hashtable();

                expected.Add(GetActorKey(), Player.Local.ActorNumber);
            }

            base.Deactivate(changes, expected);
        }

        protected override void DeactivateLocal() { }

        public void SetMinigame(int index = -1) {
            HasMinigame = index >= 0;

            if (HasMinigame) {
                _minigame = MinigameManager.Instance[index];

                if (_minigame.state.status == MinigameDescription.Status.FinSuccess ||
                    _minigame.state.status == MinigameDescription.Status.FinFailed) {
                    Debug.LogWarning("Minigame already finished");
                    
                    HasMinigame = false;
                    _indicator.SetActive(false);
                    _minigame = null;
                    return;
                }
                
                _indicator.SetActive(!InInteractRange);
                
                MinigameManager.Instance.SetMinigameLog(_minigame, this); 
                return;
            }
            
            _indicator.SetActive(false);
            _minigame = null;
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
            string key =  GetActorKey();
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
        
#if UNITY_EDITOR
        public override void OnDrawGizmosSelected() {
            if (_viewRadius > 0) {
                Color buffer = Gizmos.color;
                Gizmos.color = Color.green;
            
                Gizmos.DrawWireSphere(transform.position, _viewRadius);
            
                Gizmos.color = buffer;
            }
            
            base.OnDrawGizmosSelected();
        }
        
        public override void OnValidate() {
            base.OnValidate();
            
            if (_networked) {
                _networked = false;
                EditorUtility.SetDirty(this);

                if (!Application.isPlaying) {
                    Debug.LogWarning("Minigame interactables are never networked, minigames themselves can be set to not networked! See sceneConfig!");
                    GameObject ping = FindObjectOfType<SceneConfiguration>()?.gameObject;
                    if (ping != null) {
                        EditorGUIUtility.PingObject(ping);
                    }
                }
            }
        }
        
#endif
    }
}