using System;
using UnityEngine;
using Cohort.GameRunner.Players;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// Used to track whether the player is on the ground.
    /// </summary>
    public class GroundCheck : MonoBehaviour {
        //how far down to check, before firing falling event.
        public const float FALL_CHECK_DISTANCE = 1.0f;

        //distance to determine that player is on the ground.
        public const float GROUND_CHECK_DIST = 0.3f;

        //cast rays a bit higher, and then corrects the distance, so the raycast is not performed below the floor.
        public const float RAY_CORRECTION = 0.1f;

        public Action onGroundHit;
        public Action onFalling;
        
        public bool Grounded;
        private LayerMask _mask;

        /// <summary>   
        /// Initializes check, parented to source object.
        /// </summary>
        public void Initialize(Transform source) {
            Initialize(source, ~0);
            Initialize(source, ~LayerMask.GetMask("Player"));
        }

        /// <summary>
        /// Initializes check, parented to source object.
        /// </summary>
        /// <param name="source">parent object</param>
        /// <param name="mask">mask for filtering raycasts.</param>
        public void Initialize(Transform source, LayerMask mask) {
            _mask = mask;
            gameObject.layer = Player.LAYER;

            Grounded = true;
            transform.SetParent(source, false);
        }

        private void Update() {
            if (Grounded && GetFloorDist() > FALL_CHECK_DISTANCE) {
                onFalling?.Invoke();
                Grounded = false;
                return;
            }

            if (!Grounded && RaycastFloor(GROUND_CHECK_DIST)) {
                onGroundHit?.Invoke();
                Grounded = true;
            }
        }

        /// <summary>
        /// Resets check, so it is set to on ground state again.
        /// </summary>
        public void ResetCheck(bool invoke = false) {
            if (RaycastFloor(GROUND_CHECK_DIST)) {
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
        /// Raycast and retrieve distance to floor from current height. Infinity when no ground is found.
        /// </summary>
        public float GetFloorDist() {
            if (RaycastFloor(out RaycastHit hit)) {
                return hit.distance;
            }

            return Mathf.Infinity;
        }

        /// <summary>
        /// Raycasts to the floor, to check if there is any.
        /// </summary>
        /// <param name="maxDist">maximum distance to check downwards for.</param>
        /// <returns>True/False floor found within max distance.</returns>
        public bool RaycastFloor(float maxDist = Mathf.Infinity) {
            return RaycastFloor(out RaycastHit hit, maxDist);
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