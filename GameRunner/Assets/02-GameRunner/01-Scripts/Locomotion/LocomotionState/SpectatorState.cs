using ExitGames.Client.Photon;
using MathBuddy;
using UnityEngine;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Used for spectator mode and ghostcam mode.
    /// </summary>
    public class SpectatorState : LocomotionState
    {
        public override Locomotion.State State {
            get { return Locomotion.State.Spectator; }
        }
        
        public SpectatorState(Rigidbody rb, Locomotion lm) : base(rb, lm) { }

        public override void Enter() {
            base.Enter();
            
            _rb.isKinematic = true;
            _rb.useGravity = false;
            OnSpeedChanged(Locomotion.WALK_SPEED);
        }

        public override void Exit() {
            base.Exit();
            
            _rb.isKinematic = false;
            _rb.useGravity = true;
        }

        public override void Update() {
            base.Update();
            
            if (Direction.magnitude > FloatingPoints.LABDA) {
                //TODO_COHORT: camera
                //_target.transform.position += _cam.transform.TransformDirection(Direction) * (Speed * Time.deltaTime);
                _target.transform.position += _lm.transform.TransformDirection(Direction) * (Speed * Time.deltaTime);
            }
        }

        public override void OnPlayerPropertiesChanged(Hashtable changes, bool initialize) {
            //spectation does not network anything.
        }
    }
}