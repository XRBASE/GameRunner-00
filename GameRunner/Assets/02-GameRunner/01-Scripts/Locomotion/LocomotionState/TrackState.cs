using System;
using Cohort.Input;
using MathBuddy;
using UnityEngine;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Used to traverse local player over the track (remote is traversed using movement state).
    /// </summary>
    public class TrackState : MoveState {
        // threshold in metres before switching to the next point
        private float NEXT_POINT_THRESHOLD = 0.1f;
        
        public override Locomotion.State State {
            get { return Locomotion.State.Track; }
        }

        public WalkOnTrack Track { get; set; }
        public bool InverseDirection { get; set; }
        
        //prevents the stopping of movement when direction is set to 0 once, so auto move does not cancel the first time the player releases the arrow keys.
        public bool PreserveAutoWalk { get; set; }

        public Action onTrackEnd;
        
        private int _curPoint, _nextPoint;
        private bool _moving;
        private bool _fwd;
        private bool _endpointMoveBlock;
        
        public TrackState(Rigidbody rb, Locomotion lm) : base(rb, lm) { }

        public override void Enter() {
            _lm.StepCaster.enabled = true;
            if (_lm.Control == Locomotion.ControlType.Local) {
                InputManager.Instance.TrackInputs.cancelTrack += EndTrack;
                InputManager.Instance.TrackInputs.trackDirectionChanged += MoveOverTrack;
                InputManager.Instance.GamepadInput.leftJoystickAxisChanged += MoveOverTrack;

                InputManager.Instance.PlayerMove.runStateChanged += OnSpeedChanged;
                
                InputManager.Instance.PlayerMove.jump += Jump;
            }
            
            _lm.GroundCheck.onGroundHit += OnLand;
            _lm.GroundCheck.onFalling += OnFall;

            _curPoint = (InverseDirection) ? Track.Count - 1 : 0;
            _nextPoint = (InverseDirection) ? Track.Count - 1 : 0;
            _fwd = true;
            _moving = false;
        }

        public override void Exit()
        {
            _lm.StepCaster.enabled = false;
            if (_lm.Control == Locomotion.ControlType.Local && !InputManager.Disposed) {
                InputManager.Instance.TrackInputs.cancelTrack -= EndTrack;
                InputManager.Instance.TrackInputs.trackDirectionChanged -= MoveOverTrack;
                InputManager.Instance.GamepadInput.leftJoystickAxisChanged -= MoveOverTrack;
                
                InputManager.Instance.PlayerMove.runStateChanged -= OnSpeedChanged;
                
                InputManager.Instance.PlayerMove.jump -= Jump;
            }
            
            _lm.GroundCheck.onGroundHit -= OnLand;
            _lm.GroundCheck.onFalling -= OnFall;
            
            base.OnDirectionChanged(Vector2.zero);
            
            _nextPoint = 0;
            _curPoint = 0;
            _moving = false;
            
            PreserveAutoWalk = false;
        }
        
        private void EndTrack() {
            onTrackEnd?.Invoke();
        }

        public override void Update() {
            if (!_moving)
                return;
            
            if (_lm.Control is Locomotion.ControlType.Local or Locomotion.ControlType.NPC) {
                UpdateSpeed();
                UpdateRotation();
            }
            Move();
        }

        /// <summary>
        /// Moves target in chosen direction and picks new points or ends tracks when needed.
        /// </summary>
        private void Move() {
            Vector3 dir = Track[_nextPoint] - _target.position;
            dir.Scale(new Vector3(1, 0, 1));
            
            if (dir.magnitude < NEXT_POINT_THRESHOLD) {
                if (_nextPoint == 0 || _nextPoint == Track.Count - 1) {
                    if (!Track.finishTrackOnStartReached && (_nextPoint == 0 != InverseDirection)) {
                        OnDirectionChanged(Vector3.zero);
                        
                        _endpointMoveBlock = true;
                        return;
                    }
                    
                    EndTrack();
                    return;
                }
                SetTarget(_nextPoint, Direction.z > 0);
            }
            
            MoveTo(dir, true);
        }
        
        /// <summary>
        /// Set new target indices for next points.
        /// </summary>
        /// <param name="startIndex">current index, point from which target is moving.</param>
        /// <param name="fwd">current player direction (independent of whether the player is moving end to start, or start to end).</param>
        private void SetTarget(int startIndex, bool fwd) {
            _curPoint = startIndex;
            if (InverseDirection)
                fwd = !fwd;
            
            if (fwd) {
                _nextPoint = Mathf.Clamp(_curPoint + 1, 0, Track.Count - 1);
            }
            else {
                _nextPoint = Mathf.Clamp(_curPoint - 1, 0, Track.Count - 1);
            }
        }

        public void MoveOverTrack(Vector2 direction) { 
            if (Mathf.Abs(direction.y) > FloatingPoints.LABDA) {
                if (_endpointMoveBlock || (_moving && _fwd == direction.y > FloatingPoints.LABDA)) {
                    return;
                }
            }
            else if (!_endpointMoveBlock && !_moving) {
                return;
            }
            
            MoveOverTrack(direction.y);
        }

        /// <summary>
        /// Moves target (using arrow keys) over track.
        /// </summary>
        /// <param name="direction">direction forwards or backwards to traverse.</param>
        public void MoveOverTrack(float direction) {
            if (PreserveAutoWalk && direction < FloatingPoints.LABDA) {
                PreserveAutoWalk = false;
                return;
            }
            
            OnDirectionChanged(new Vector2(0f, direction));
        }

        public override void OnDirectionChanged(Vector2 direction) {
            if (Mathf.Abs(direction.y) <= FloatingPoints.LABDA) {
                _moving = false;
                base.OnDirectionChanged(direction);
                
                _endpointMoveBlock = false;
                return;
            }
            
            base.OnDirectionChanged(direction);
            
            _moving = true;
            bool flip = _fwd != Direction.z > 0; 
            _fwd = Direction.z > 0;
            
            if (!flip) {
                SetTarget(_curPoint, _fwd);
            }
            else {
                SetTarget(_nextPoint, _fwd);
            }
        }
    }
}