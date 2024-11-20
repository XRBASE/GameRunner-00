using System.Collections.Generic;
using Cohort.Config;
using Cohort.CustomAttributes;
using Cohort.Users;
using MathBuddy.Flags;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cohort.GameRunner.Input {
    /// <summary>
    /// Adapter/wrapper class around pointer input to translate action in raycasts and handle dragging.
    /// </summary>
//after prefabloader subscribtion to login event
    [DefaultExecutionOrder(1)]
    public class CursorRayCaster : MonoBehaviour {
        public const float MAX_DIST = 60f;

        //angles higher than this threshold are not viable for navigation.
        private const float NAVIGATE_ANGLE_THRESHOLD = 60;
        private readonly LayerMask MASK = ~(1 << InputConfig.IGNORE_RAYCAST_LAYER);

        /// <summary>
        /// The state of the last raycast (updated each frame)
        /// </summary>
        public RCHitState HitState {
            get { return _hitstate; }
            set { _hitstate = value; }
        }

        [ReadOnly, SerializeField] private RCHitState _hitstate; 

        /// <summary>
        /// The last made raycast hit (can be default if no hit was found, updated each frame). 
        /// </summary>
        public RaycastHit CurHit {
            get { return _curHit; }
            private set { _curHit = value; }
        }

        private RaycastHit _curHit;

        public Ray CurRay {
            get { return _curRay; }
            set { _curRay = value; }
        }

        private Ray _curRay;

        public int curHitLayer;

        /// <summary>
        /// True when the pointer is over an UI element
        /// </summary>
        public bool PointerOverUI {
            get { return HitState == RCHitState.UI; }
        }

        private Camera _mainCam;

        //true when logged in to ensure that the main camera is already set
        //TODO: this might be manageble without this call if some scripts are moved into the player class,
        //not sure if we should do this
        private bool _initialized;

        public void Awake() {
            HitState = RCHitState.None;
            DataServices.Login.onUserLoggedIn += OnLogin;
        }

        public void OnDestroy() {
            DataServices.Login.onUserLoggedIn -= OnLogin;
        }

        private void OnLogin(User local) {
            _mainCam = Camera.main;
            _initialized = true;
        }

        public bool MouseOverUI() {
            List<RaycastResult> evtHits = GetEventSystemRaycastResults();
            return evtHits.Count > 0;
        }

        ///Gets all event systen raycast results of current mouse or touch position.
        static List<RaycastResult> GetEventSystemRaycastResults() {
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = UnityEngine.Input.mousePosition;

            List<RaycastResult> raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);
            return raysastResults;
        }

        private void Update() {
            if (!_initialized) return;

            //cache racasthit in update, so cur als always up to date.
            UpdateCurRaycast();
        }

        /// <summary>
        /// Called to update the hitstate and the raycasthit
        /// </summary>
        private void UpdateCurRaycast() {
            //if user is pointing at UI, no hit is needed.
            if (MouseOverUI()) {
                _curHit = default(RaycastHit);
                HitState = RCHitState.UI;
                return;
            }

            CurRay = _mainCam.ScreenPointToRay(InputManager.Instance.Cursor.ScreenPosition);
            if (Physics.Raycast(CurRay, out _curHit, MAX_DIST, MASK, QueryTriggerInteraction.Ignore)) {
                curHitLayer = _curHit.collider.gameObject.layer;
                if (InputConfig.Config.interactionLayerMask.MaskIncludes(curHitLayer)) {
                    //check layer, interact first, then move
                    HitState = RCHitState.Interact;
                    return;
                }

                if (Vector3.Angle(Vector3.up, _curHit.normal) <=
                    NAVIGATE_ANGLE_THRESHOLD) {

                    HitState = RCHitState.Navigate;
                    curHitLayer = _curHit.collider.gameObject.layer;
                    return;
                }

                //no hit, return state none
                HitState = RCHitState.Other;
                return;
            }

            HitState = RCHitState.None;
            _curHit = default(RaycastHit);
            curHitLayer = default(LayerMask);
        }

        /// <summary>
        /// The result type of a raycast in the world, used for checking which actions are to be called.
        /// </summary>
        public enum RCHitState {
            None = 0,
            Interact = 1,
            Navigate = 2,
            UI = 3,
            Other = 4,
        }
    }
}