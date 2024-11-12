using System;
using UnityEngine;
using Cohort.GameRunner.Players;

namespace Cohort.GameRunner.LocoMovement
{
    /// <summary>
    /// Used to track whether the player is on the ground.
    /// </summary>
    public class GroundCheck : MonoBehaviour
    {
        //cast rays a bit higher, and then corrects the distance, so the raycast is not performed below the floor.
        public const float RAY_CORRECTION = 0.1f;

        public Action onGroundHit;
        public Action onFalling;

        public bool Grounded= true;
        private LayerMask _mask;
        private FloorCollision _floorCollision;
        private bool _hasFloorCollision;


        /// <summary>
        /// Initializes check, parented to source object.
        /// </summary>
        /// <param name="source">parent object</param>
        /// <param name="mask">mask for filtering raycasts.</param>
        public void Initialize(Transform source, FloorCollision floorCollision)
        {
            _mask = ~LayerMask.GetMask("Player");
            floorCollision.Initialise(_mask);
            floorCollision.onFloorCollision += HandleFloorCollision;
            gameObject.layer = Player.LAYER;
            transform.SetParent(source, false);
        }

        private void HandleFloorCollision(bool hasFloorCollision)
        {
            if(Grounded == hasFloorCollision)
                return;
            
            if (hasFloorCollision)
                onGroundHit?.Invoke();
            else
                onFalling?.Invoke();
            
            Grounded = hasFloorCollision;
        }



        /// <summary>
        /// Raycasts to the floor, to check if there is any.
        /// </summary>
        /// <param name="hit">out variable for found raycast hit.</param>
        /// <param name="maxDist">maximum distance to cast downwards.</param>
        /// <returns>True/False floor found within max distance.</returns>
        public bool RaycastFloor(out RaycastHit hit, float maxDist = Mathf.Infinity)
        {
            if (Physics.Raycast(transform.parent.position + Vector3.up * RAY_CORRECTION, Vector3.down, out hit, maxDist,
                _mask, QueryTriggerInteraction.Ignore))
            {
                hit.distance -= RAY_CORRECTION;
                return true;
            }

            return false;
        }
    }
}