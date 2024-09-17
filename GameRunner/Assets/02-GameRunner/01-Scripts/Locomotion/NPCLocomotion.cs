using UnityEngine;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Locomotion system that specifically is used for NPC's.
    /// </summary>
    public class NPCLocomotion : Locomotion {
        public override ControlType Control {
            get { return ControlType.NPC; }
        }

        public override bool Networked {
            get { return false; }
        }
        
        protected override void Awake() {
            //create components that usually would be contained in the player prefab.
            _rb = gameObject.AddComponent<Rigidbody>();
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            
            _collider = gameObject.AddComponent<CapsuleCollider>();
            _collider.height = 1.6f;
            _collider.radius = 0.2f;
            _collider.center = Vector3.up * 0.875f;
            
            base.Awake();
        }
        
        /// <summary>
        /// Make target jump.
        /// </summary>
        public void Jump() {
            ((MoveState)_sm.Current).Jump();
        }
        
        /// <summary>
        /// Stop current movement coroutines.
        /// </summary>
        public void StopMoving() {
            ((MoveState)_sm.Current).StopMoving();
        }
    }
}