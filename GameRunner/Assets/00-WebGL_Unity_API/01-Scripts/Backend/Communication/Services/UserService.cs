using System;
using Cohort.Tools.Timers;
using Cohort.Users;
using UnityEngine;
using UnityEngine.Events;

namespace Cohort.BackendData.DataService
{
    public class UserService : MonoBehaviour
    {
        //Intervals between role updates 
        private const float UPDATE_DURATION = 60f;
        
        /// <summary>
        /// Most recent entry of local user, is updated, but won't be removed like cache will
        /// </summary>
        public User Local {
            get { return _local; }
            set {
                if (_local != null) {
                    if (_local.Overwrite(value)) {
                        //local changed
                        onLocalUserUpdated?.Invoke(_local);
                    }
                }
                else {
                    _local = value;
                    onLocalUserUpdated?.Invoke(_local);
                }
                
            }
        }
        private User _local;
        
        public UnityAction<User> onLocalUserUpdated;
        public UserRepository repo;
        
        private Timer _updateTimer;
        private bool _autoUpdate = false;
        
        private void Awake()
        {
            repo = new UserRepository();
            _updateTimer = new Timer(UPDATE_DURATION, false, UpdateLocalUser);
        }

        private void Start()
        {
            DataServices.Login.onUserLoggedOut += OnLogout;
        }

        public void StartAutoUserUpdate() {
            if (!_autoUpdate && !_updateTimer.Active) {
                _updateTimer.Reset();
                _updateTimer.Start();
            }
            
            _autoUpdate = true;
        }

        public void StopAutoUserUpdate() {
            if (_autoUpdate && _updateTimer.Active) {
                _updateTimer.Stop();
            }
            
            _autoUpdate = false;
        }

        public void UpdateLocalUser() {
            _updateTimer.Reset();
            if (_autoUpdate && !_updateTimer.Active) {
                _updateTimer.Start();
            }
            
            GetLocal();
        }
        
        /// <summary>
        /// Internal logout call
        /// </summary>
        private void OnLogout()
        {
            _local = null;
            StopAllCoroutines();
        }

        /// <summary>
        /// Get local data from backend.
        /// </summary>
        public void GetLocal(UnityAction<User> onComplete = null, UnityAction<string> onFailure = null)
        {
            //if local is null the cache will be skipped, otherwise cache will still be used.
            StartCoroutine(repo.RetrieveLocal(onComplete, onFailure));
        }
        
        /// <summary>
        /// Get data of specific user from backend.
        /// </summary>
        /// <param name="user">user of which to retrieve/overwrite data (minimally only with userId).</param>
        public void GetUser(string uuid, UnityAction<User> onComplete, UnityAction<string> onFailed = null) {
            throw new Exception("Missing get user data call.");
            //StartCoroutine(repo.RetrieveUser(uuid, onComplete, onFailed));
        }

        public void SetAvatarData(string avatarData, Action onComplete = null) {
            StartCoroutine(repo.SetLocalUserAvatarData(avatarData, onComplete));
        }
    }
}