//Old GroundCheck
using System;
using UnityEngine;
using Cohort.GameRunner.Players;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Used to track whether the player is on the ground.
    /// </summary>
    public class GroundCheck : MonoBehaviour {
        //cast rays a bit higher, and then corrects the distance, so the raycast is not performed below the floor.
        public const float RAY_CORRECTION = 0.1f;

        public Action onGroundHit;
        public Action onFalling;
        
        public bool Grounded;
        private LayerMask _mask;
        private FloorCollision _floorCollision;
        private bool _hasGroundCollision;
        private bool _hasFallCollision;


        /// <summary>
        /// Initializes check, parented to source object.
        /// </summary>
        /// <param name="source">parent object</param>
        /// <param name="mask">mask for filtering raycasts.</param>
        public void Initialize(Transform source, FloorCollision GroundChecker, FloorCollision FallChecker) {
            _mask = ~LayerMask.GetMask("Player");
            gameObject.layer = Player.LAYER;
            GroundChecker.Initialise(_mask, new Vector3(.5f,.4f,.5f), Vector3.down* .1f );
            FallChecker.Initialise(_mask, new Vector3(.5f,1f,.5f), Vector3.down* .4f);
            GroundChecker.onFloorCollision += HandleGroundCheck;
            FallChecker.onFloorCollision += HandleFallCheck;
            Grounded = true;
            transform.SetParent(source, false);
        }
        
        private void HandleGroundCheck(bool isColliding)
        {
            _hasGroundCollision = isColliding;
        }
        
        private void HandleFallCheck(bool isColliding)
        {
            _hasFallCollision = isColliding;
        }
        
        private void Update() {
            if (Grounded && !_hasFallCollision) {
                onFalling?.Invoke();
                Grounded = false;
                return;
            }
            if (!Grounded && _hasGroundCollision) {
                onGroundHit?.Invoke();
                Grounded = true;
            }
        }

        /// <summary>
        /// Resets check, so it is set to on ground state again.
        /// </summary>
        public void ResetCheck(bool invoke = false) {
            if (_hasGroundCollision) {
                Grounded = true;

                if (invoke) {
                    onGroundHit?.Invoke();
                }
                return;
            }
            
            if (invoke) {
                onFalling?.Invoke();
            }
            Grounded = false;
        }

        /// <summary>
        /// Raycasts to the floor, to check if there is any.
        /// </summary>
        /// <param name="hit">out variable for found raycast hit.</param>
        /// <param name="maxDist">maximum distance to cast downwards.</param>
        /// <returns>True/False floor found within max distance.</returns>
        public bool RaycastFloor(out RaycastHit hit, float maxDist = Mathf.Infinity) {
            if (Physics.Raycast(transform.parent.position + Vector3.up * RAY_CORRECTION, Vector3.down, out hit, maxDist,
                                _mask, QueryTriggerInteraction.Ignore)) {
                hit.distance -= RAY_CORRECTION;
                return true;
            }

            return false;
        }
    }
}