using Cohort.GameRunner.AvatarAnimations;
using Cohort.Networking.PhotonKeys;
using Cohort.Ravel.Patterns.States;
using Cohort.GameRunner.Players;
using Cohort.CustomAttributes;
using Cohort.GameRunner.Input;
using ExitGames.Client.Photon;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

using Avatar = Cohort.GameRunner.Avatars.Avatar;

namespace Cohort.GameRunner.LocoMovement {
    public abstract class Locomotion : MonoBehaviour
    {
        public static Locomotion Local {
            get;
            protected set;
        }

        //respawns after falling this amount of meters.
        public const float FALL_RESET_HEIGHT = 100f;
        
        //difference between speed of remote and local players
        public const float REMOTE_DIFF = -0.5f;
        
        //height in meters of one singular jump
        public const float JUMP_HEIGHT = 2f;
        
        //speeds in meters per second
        public const float WALK_SPEED = 3.5f;
        public const float RUN_SPEED = 6f;
        
        //increment in speed, in meters per second, until target speed is reached.
        public const float SPEED_INC = 7.5f;
        //rotation in degrees per seconds
        public const float ROT_SPEED = 1250f;
        //rotation in degrees per seconds when using QE keys
        public const float ROT_INPUT_SPEED = 200f;
        
        //Runs when distance is higher than this
        public const float RUN_THRESHOLD = 5f;
        //Teleports when distance is higher than this
        public const float TELEPORT_THRESHOLD = 15f;
        //teleports after calculated walk time + this threshold has been spend trying to reach a locotion,
        //but the location has not been reached.
        public const float TIMEOUT_THRESHOLD = 0.9f;

        public abstract ControlType Control { get; }

        public virtual bool Networked {
            get { return true; }
        }
        
        public bool Seated { get; private set; }

        public CharAnimator Animator { get; private set; }
        public GroundCheck GroundCheck { get; private set; }
        public StepRaycaster StepCaster { get; private set; }
        
        public FloorCollision GroundChecker { get; private set; }
        public FloorCollision FallChecker { get; private set; }

        public Action onSitDown, onStandUp;
        public static Action<Player> onColliderDisable;
        
        [SerializeField, ReadOnly] protected State _state;
        
        protected StateMachine<State, LocomotionState> _sm;
        
        protected string _key;
        protected Hashtable _changeTable;
        protected bool _hasChanges;

        protected Rigidbody _rb;
        protected CapsuleCollider _collider;
        protected Player _player;

        private bool _initialized = false;
        
        protected virtual void Awake() {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();
            if (_collider == null)
                _collider = GetComponent<CapsuleCollider>();
            
            GroundChecker = new GameObject("GroundChecker").AddComponent<FloorCollision>();
            GroundChecker.transform.parent = transform;
            
            FallChecker = new GameObject("FallChecker").AddComponent<FloorCollision>();
            FallChecker.transform.parent = transform;

            GroundCheck = new GameObject("GroundCheck").AddComponent<GroundCheck>();
            GroundCheck.gameObject.layer = gameObject.layer;
            GroundCheck.Initialize(transform, GroundChecker, FallChecker);
            
            StepCaster = new GameObject("StepCaster").AddComponent<StepRaycaster>();
            StepCaster.gameObject.layer = gameObject.layer;
            StepCaster.Initialize(_rb, this);
            
            
            _sm = new StateMachine<State, LocomotionState>();
            _sm[State.Move] = new MoveState(_rb, this);
            _sm[State.Seat] = new SeatState(_rb, this);

            if (Control != ControlType.NPC) {
                _sm[State.Spectator] = new SpectatorState(_rb, this);
                _sm[State.Track] = new TrackState(_rb, this);
            }
            
            _changeTable = new Hashtable();
            
            _player = GetComponent<Player>();
            _player.onAvatarImported += SetUpNewAvatar;
            if (_player.Avatar != null) {
                SetUpNewAvatar(_player.Avatar);
            }
        }
        
        protected virtual void Start() {
            if (Networked) {
                Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
                _player.onPropertiesChanged += OnCustomPropertiesChanged;
            }

            if (Control != ControlType.NPC) {
                ((TrackState)_sm[State.Track]).onTrackEnd += OnTrackEnd;
            }
            
            if (Control == ControlType.Local) {
                EnvironmentLoader.Instance.onEnvironmentLoaded += InitLocomotion;
            }
            
            if (Networked && Network.Local.Client.InRoom) {
                OnJoinedRoom();
            }
        }
        
        protected virtual void OnDestroy() {
            if (Control != ControlType.NPC) {
                ((TrackState)_sm[State.Track]).onTrackEnd -= OnTrackEnd;
            }

            if (Networked) {
                _player.onPropertiesChanged -= OnCustomPropertiesChanged;
                Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
            }
            
            if (Control == ControlType.Local) {
                EnvironmentLoader.Instance.onEnvironmentLoaded -= InitLocomotion;
            }
        }
        
        /// <summary>
        /// Assign new avatar to this locomotion class.
        /// </summary>
        public void SetUpNewAvatar(Avatar avatar) {
            CharAnimator newAnim = avatar.GetComponent<CharAnimator>();
            newAnim.CopyFrom(Animator);

            Animator = newAnim;
            if (Control == ControlType.Local) {
                //TODO_COHORT: Camerastate
                //CameraState.Instance.PlayerFocusTransform = Animator.GetBone(HumanBodyBones.Head);
            }
            else if (!_initialized) {
                InitLocomotion();
            }
        }
        
        /// <summary>
        /// Update network data when joining a room
        /// </summary>
        protected virtual void OnJoinedRoom() {
            OnCustomPropertiesChanged(_player.CustomProperties, true);
        }
        
        /// <summary>
        /// Called whenever room properties are updated.
        /// </summary>
        /// <param name="changes">Changes in room properties.</param>
        private void OnCustomPropertiesChanged(Hashtable changes) {
            OnCustomPropertiesChanged(changes, false);
        }
        
        /// <summary>
        /// Called whenever room properties are updated.
        /// </summary>
        /// <param name="changes">Changes in room properties.</param>
        /// <param name="initialize">Are values being initialized (join room calls).</param>
        protected virtual void OnCustomPropertiesChanged(Hashtable changes, bool initialize) { }
        
        /// <summary>
        /// Make target look in a specific direction.
        /// </summary>
        public void LookAt(Vector3 direction) {
            if (_sm.State == State.Move) {
                ((MoveState)_sm.Current).LookInDirection(direction, true, true);
            }
        }
        
        /// <summary>
        /// Make target move to a given location.
        /// </summary>
        public void MoveTo(Vector3 position) {
            ((MoveState)_sm.Current).MoveToPosition(position);
        }

        /// <summary>
        /// Make target move to a given location with a specific movement state
        /// </summary>
        public void MoveTo(Vector3 position, MoveState.MovementType movementType)
        {
            ((MoveState)_sm.Current).MoveToPosition(position, movementType);
        }
        
        /// <summary>
        /// Make target teleport to a given location and rotation.
        /// </summary>
        public void TeleportTo(Vector3 pos, Quaternion rot) {
            ((MoveState)_sm.Current).TeleportToPosition(pos, rot);
        }
        
        /// <summary>
        /// Teleport target to default spawn position.
        /// </summary>
        private void TeleportToSpawn() {
            if (_sm.State != State.Move) {
                OnLocalStateChange(State.Move);
                Animator.ForceEnterState(CharAnimator.AnimationState.Idle);
                Seated = false;
            }

            if (SpawnPoint.TryGetById(SpawnPoint.DEFAULT, out SpawnPoint spawn)) {
                spawn.TeleportToSpawnPoint();
            }
            else {
                TeleportTo(Vector3.zero, Quaternion.identity);
            }
        }

        private void InitLocomotion() {
            if (Control == ControlType.Local) {
                TeleportToSpawn();
            }
            
            ActivateRigidBody();
            
            _sm.State = State.Move;
            _state = State.Move;
            
            _initialized = true;
        }
        
        /// <summary>
        /// Activates the rigidbody and gravity.
        /// </summary>
        public virtual void ActivateRigidBody() {
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }
        
        /// <summary>
        /// Deactivates the rigidbody and gravity.
        /// </summary>
        public virtual void DeactivateRigidBody() {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        protected void Update() {
            _sm.Update();
        }

        protected virtual void FixedUpdate() {
            //publish changes whenever there are new changes.
            if (Networked && _hasChanges && _player.Initialized && Network.Local.Client.IsConnectedAndReady) {
                _player.SetCustomProperties(_changeTable);
                _changeTable.Clear();
                _hasChanges = false;
            }
        }
        
        /// <summary>
        /// Networks locomotion state changes.
        /// </summary>
        private void OnLocalStateChange(State newState, bool network = true) {
            _sm.State = newState;
            _state = newState;

            if (!Networked && network)
                return;
            
            _key = Keys.Get(Keys.Player.LocomotionState);
            _changeTable[_key] = (int)newState;
            _hasChanges = true;
        }
        
        /// <summary>
        /// Network animation state change.
        /// </summary>
        public void NetworkJump(CharAnimator.AnimationState state) {
            if (!Networked)
                return;
            
            _key = Keys.Get(Keys.Player.Jump);
            _changeTable[_key] = state;
            
            _key = Keys.Get(Keys.Player.JumpExpire);
            //0.3 is approximately the time it takes to play the jump animation
            _changeTable[_key] = TimeManager.Instance.RefTime + 0.3f;
            
            _hasChanges = true;
        }
        
        /// <summary>
        /// Make target sit down on given transform position.
        /// </summary>
        public void SitDown(Transform seat) {
            if (Animator.State == CharAnimator.AnimationState.EmoteFullBody) {
                Animator.ForceExitEmote();
            }
            
            if (Seated) {
                OnStandUp(false);
                ((SeatState)_sm[State.Seat]).Seat = seat;
                Vector3 pos = ((SeatState)_sm[State.Seat]).SeatPosition;
                transform.position = pos;
                
                ((SeatState)_sm[State.Seat]).LookInDirection(((SeatState)_sm[State.Seat]).Seat.forward, true, true);
                
                OnSitDown(false);
            }
            else if (_sm.State == State.Move) {
                ((SeatState)_sm[State.Seat]).Seat = seat;
                ((MoveState)_sm.Current).MoveToPosition(((SeatState)_sm[State.Seat]).SeatPosition, OnSitDown);
            }
            Seated = true;
        }
        
        public void OnStandUp() {
            OnStandUp(true);
        }

        public virtual void OnSitDown() {
            OnSitDown(true);
        }

        /// <summary>
        /// Called when the target is actually sitting down (the moment but touches the chair).
        /// </summary>
        public virtual void OnSitDown(bool networkState) {
            OnLocalStateChange(State.Seat, networkState);
            
            onSitDown?.Invoke();
        }
        
        /// <summary>
        /// Called when the target has actually stood up.
        /// </summary>
        public void OnStandUp(bool networkState) {
            onStandUp?.Invoke();
            
            Seated = false;
            OnLocalStateChange(State.Move, networkState);
        }
        
        /// <summary>
        /// Start the track state, causing the locomotion to follow the track given.
        /// </summary>
        /// <param name="track">track that is being followed.</param>
        /// <param name="inverse">Is the track started from the first, or last point.</param>
        public void FollowTrack(WalkOnTrack track, bool inverse) {
            if (_sm.State == State.Track && ((TrackState)_sm[State.Track]).Track == track) {
                return;
            }
            
            ((TrackState)_sm[State.Track]).Track = track;
            ((TrackState)_sm[State.Track]).InverseDirection = inverse;

            if (Control == ControlType.Local) {
                Vector3 dir;
                float speed;
                if (((MoveState)_sm[State.Move]).UsingCoroutine) {
                    dir = Vector3.zero;
                    if (!track.autoWalk) {
                        speed = 0f;
                    }
                    else {
                        speed = Mathf.Max(_sm.Current.Speed, WALK_SPEED);
                        dir = Vector3.forward;
                    }
                }
                else {
                    dir = _sm.Current.Direction;
                    if (!track.autoWalk) {
                        speed = _sm.Current.Speed;
                    }
                    else {
                        speed = Mathf.Max(_sm.Current.Speed, WALK_SPEED);
                        dir = Vector3.forward;
                        ((TrackState)_sm[State.Track]).PreserveAutoWalk = true;
                    }
                }
                
                _sm.State = State.Track;
                _state = State.Track;
                _sm.Current.InitValues(dir, speed);
            }
            else {
                _sm.State = State.Track;
                _state = State.Track;
            }
        }
        
        /// <summary>
        /// Called whenever the player leaves a track.
        /// </summary>
        public void OnTrackEnd() {
            if (Control == ControlType.Local) {
                Vector3 dir;
                float speed;
                if (InputManager.Instance.Cursor.LeftDown || InputManager.Instance.Cursor.RightDown || ((TrackState)_sm[State.Track]).Track.autoWalk) {
                    dir = Vector3.zero;
                    speed = 0f;
                }
                else {
                    dir = _sm.Current.Direction;
                    speed = _sm.Current.Speed;
                }
                
                _sm.State = State.Move;
                _state = State.Move;
                _sm.Current.InitValues(dir, speed);
            }
            else {
                _sm.State = State.Move;
                _state = State.Move;
            }
            ((TrackState)_sm[State.Track]).Track = null;
        }

        public enum State
        {
            Move,
            Seat,
            Spectator,
            Track,
        }

        public enum ControlType {
            Local,
            Remote,
            NPC
        }
    }
}