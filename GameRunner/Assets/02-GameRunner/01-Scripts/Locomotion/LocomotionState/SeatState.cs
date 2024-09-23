using Cohort.GameRunner.AvatarAnimations;
using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Input;
using System.Collections;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Locomotion state that handles the player sitting down, sitting and standing up.
    /// </summary>
    public class SeatState : LocomotionState
    {
        //offset used to wait between reaching a location and starting the sit down animation
        public const float SIT_DOWN_DUR = 2.6f;
        //duration used to wait after invoking stand up and switching back to the move state.
        public const float STAND_UP_DUR = 0.90f;
        
        //offset required for but to be placed on the seat position of the chair.
        private readonly Vector3 BUTT_OFFSET = new Vector3(0, -0.6f, 0.6f);
        
        public override Locomotion.State State {
            get { return Locomotion.State.Move; }
        }
        
        public Transform Seat { get; set; }
        public Vector3 SeatPosition {
            get { return Seat.position + Seat.TransformDirection(BUTT_OFFSET); }
        }

        //used for sitting down and standing up coroutines.
        private Coroutine _routine;
        
        public SeatState(Rigidbody rb, Locomotion lm) : base(rb, lm) { }
        
        public override void Enter()
        {
            if (_routine == null) {
                _routine = _lm.StartCoroutine(DoSitDown());
            }
            
            _lm.DeactivateRigidBody();
            
            _rb.isKinematic = true;
            _target.transform.position = SeatPosition;
            
            if (_lm.Control == Locomotion.ControlType.Local)
            {
                InputManager.Instance.PlayerMove.moveToCursor += ExitState;
                InputManager.Instance.PlayerMove.teleportToCursor += ExitState;

                InputManager.Instance.GamepadInput.leftJoystickAxisChanged += ExitState;
                InputManager.Instance.GamepadInput.rightDownButtonPressed += ExitState;
            }
        }

        public override void Exit() {
            Seat = null;
            if (_routine != null) {
                _lm.StopCoroutine(_routine);
                _routine = null;
            }
            
            if (_lm.Control == Locomotion.ControlType.Local)
            {
                InputManager.Instance.PlayerMove.moveToCursor -= ExitState;
                InputManager.Instance.PlayerMove.teleportToCursor -= ExitState;
                
                InputManager.Instance.GamepadInput.leftJoystickAxisChanged -= ExitState;
                InputManager.Instance.GamepadInput.rightDownButtonPressed -= ExitState;
            }
            
            _rb.isKinematic = false;
        }
        
        public override void OnPlayerPropertiesChanged(Hashtable changes, bool initialize) {
            string key = Keys.Get(Keys.Player.Position);
            if (_lm.Control != Locomotion.ControlType.Local && changes.ContainsKey(key) && changes[key] != null) {
                _target.transform.position = (Vector3)changes[key];
            }
			
            base.OnPlayerPropertiesChanged(changes, initialize);
        }

        private void ExitState(bool pressed) { ExitState(); }
        private void ExitState(Vector2 dir) { ExitState(); }

        private void ExitState(Vector3 dir) { ExitState(); }
        
        /// <summary>
        /// Called to invoke standing up
        /// </summary>
        public void ExitState() {
            if (_routine == null) {
                _routine = _lm.StartCoroutine(DoStandUp());
            }
        }
        
        private IEnumerator DoSitDown()
        {
            LookInDirection(Seat.forward, true, true);
            
            _lm.Animator.ForceEnterState(CharAnimator.AnimationState.SitDown);
            yield return new WaitForSeconds(SIT_DOWN_DUR);
            
            _lm.OnSitDown();
            _routine = null;
        }
        
        private IEnumerator DoStandUp() {
            if (_lm.Animator.SetState(CharAnimator.AnimationState.StandUp)) {
                yield return new WaitForSeconds(STAND_UP_DUR);
            
                _lm.ActivateRigidBody();
                _lm.OnStandUp();
                
                _routine = null;
            }
        }
    }
}
