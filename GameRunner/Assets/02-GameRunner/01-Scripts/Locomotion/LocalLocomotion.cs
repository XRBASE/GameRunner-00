using Cohort.GameRunner.AvatarAnimations;
using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Input;
using UnityEngine;
using System;

namespace Cohort.GameRunner.LocoMovement {
    /// <summary>
    /// This will be shell on top of the locomotion class that applies the input of the player.
    /// The locomotion class uses this input to in term control the behavior and position of the player.
    /// </summary>
    public class LocalLocomotion : Locomotion {
        //updates position on network if more than this distance is traversed.
        private const float POS_UPDATE_THRE = 0.2f;
        //updates rotation if more that this rotation difference is met.
        private const float ROT_UPDATE_THRE = 15f;

        /// <summary>
        /// Determines what type of control this locomotion object has.
        /// </summary>
        public override ControlType Control {
            get { return ControlType.Local; }
        }
        
        //player translation values are cached in this matrix.
        private Matrix4x4 _prevTrans;
        
        protected override void Awake() {
            base.Awake();
            
            _prevTrans = Matrix4x4.identity;
            Local = this;
        }

        protected override void Start() {
            base.Start();
            
            //TODO_COHORT: camera
            //CameraState.Instance.onGhostCam += OnSpectate;
            InputManager.Instance.EmoteInput.emote += OnEmote;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            
            //TODO_COHORT: camera
            //CameraState.Instance.onGhostCam -= OnSpectate;
            if (!InputManager.Disposed) {
                InputManager.Instance.EmoteInput.emote -= OnEmote;
            }
        }

        protected override void OnJoinedRoom() { }

        protected override void UpdateNetwork() {
            if ((transform.position - _prevTrans.GetPosition()).magnitude >= POS_UPDATE_THRE) {
                SendPosition();
            }

            if (Math.Abs(transform.rotation.eulerAngles.y - _prevTrans.rotation.eulerAngles.y) >= ROT_UPDATE_THRE) {
                SendRotation();
            }
            
            base.UpdateNetwork();
        }

        /// <summary>
        /// Send position of player on the network.
        /// </summary>
        private void SendPosition() {
            _changeTable[Keys.Get(Keys.Player.Position)] = transform.position;
            Vector3 pos = transform.position;
            if (_sm.State == State.Move) {
                pos += new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            }
            
            _prevTrans.SetTRS(pos, _prevTrans.rotation, _prevTrans.lossyScale);
            _hasChanges = true;
        }
        
        /// <summary>
        /// Send rotation of player on the network.
        /// </summary>
        public void SendRotation() {
            _changeTable[Keys.Get(Keys.Player.Rotation)] = transform.rotation;
            _prevTrans.SetTRS(_prevTrans.GetPosition(), transform.rotation, _prevTrans.lossyScale);
            _hasChanges = true;
        }

        //update position and location when sitting down independent of the position and rotation thresholds.
        public override void OnSitDown(bool networkState = true) {
            base.OnSitDown(networkState);
            
            SendPosition();
            SendRotation();
        }
        
        /// <summary>
        /// Send emote call over the network.
        /// </summary>
        /// <param name="emoteIndex">index of emote in the emoteset.</param>
        protected void OnEmote(int emoteIndex) {
            if (Seated || !_player.Visible)
                return;
            
            if (Animator.DoEmote(emoteIndex, out Emote emote)) {
                //network guid
                if (Control == ControlType.Local) {
                    string key = Keys.Get(Keys.Player.Emote);
                    _changeTable[key] = emote.GUID;
                    
                    //expire is used to check when the emote was played.
                    key = Keys.Get(Keys.Player.EmoteExpire);
                    _changeTable[key] = TimeManager.Instance.RefTime + emote.clip.length;
                    _hasChanges = true;
                }
            }
        }
        
        /// <summary>
        /// Enters spectate and ghostcam mode for the target.
        /// </summary>
        /// <param name="active">True/False spectate mode entered.</param>
        public void OnSpectate(bool active) {
            if (active) {
                if (Seated) {
                    OnStandUp();
                }
            
                _sm.State = State.Spectator;
                _state = State.Spectator;
                
                onColliderDisable?.Invoke(_player);
            }
            else {
                _sm.State = State.Move;
                _state = State.Move;

                Vector3 pos; 
                if (GroundCheck.RaycastFloor(out RaycastHit hit)) {
                    pos = hit.point;
                }
                else {
                    pos = SpawnPoint.DefaultSpawnPosition();
                }
                
                ((MoveState)_sm.Current).TeleportToPosition(pos);
            }

            _collider.enabled = !active;
        }
    }
}