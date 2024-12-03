using System;
using System.Collections.Generic;
using Cohort.Patterns;
using UnityEngine;

namespace Cohort.Tools.Timers
{
    public class TimerManager : Singleton<TimerManager>
    {
        private HashSet<Timer> _timers;
        //this only temporarily buffers elapsed timers so they can be removed.
        private HashSet<Timer> _elapsedTimers;
        private HashSet<Timer> _newTimers;

        private HashSet<StopWatch> _stopWatches;
        private HashSet<StopWatch> _newWatches;

        protected override void Awake()
        {
            base.Awake();
            //start both sets at some capacity, so there is some room for them to grow, without the need of a size increment
            _timers = new HashSet<Timer>(8);
            _stopWatches = new HashSet<StopWatch>(8);
            
            _elapsedTimers = new HashSet<Timer>(4);
            _newTimers = new HashSet<Timer>(4);
            
            //stopwatches does not need a elapsed, because they are not changed during the update loop
            _newWatches = new HashSet<StopWatch>(4);
        }

        private void Update()
        {
            foreach (Timer newt in _newTimers) {
                if (!_timers.Contains(newt)) {
                    _timers.Add(newt);
                }
            }
            _newTimers.Clear();
            
            foreach (Timer timer in _timers) {
                if (timer.Update(Time.deltaTime)) {
                    //if timer is reset on the onComplete call, this prevents the timer from being removed
                    if (timer.HasFinished) {
                        _elapsedTimers.Add(timer);
                    }
                }
            }
            
            foreach (Timer elapsed in _elapsedTimers) {
                RemoveTimer(elapsed);
            }
            _elapsedTimers.Clear();

            foreach (StopWatch neww in _newWatches) {
                if (!_stopWatches.Contains(neww)) {
                    _stopWatches.Add(neww);
                }
            }
            _newWatches.Clear();
            
            foreach (var stopWatch in _stopWatches) {
                stopWatch.Update(Time.deltaTime);
            }
        }

        public void AddTimer(Timer t) {
            if (!_timers.Contains(t)) {
                _newTimers.Add(t);
            }
        }

        public void RemoveTimer(Timer t)
        {
            if (_timers.Contains(t)) {
                _timers.Remove(t);
            }
        }

        public void AddStopWatch(StopWatch sw) {
            _newWatches.Add(sw);
        }

        public void RemoveStopWatch(StopWatch sw)
        {
            if (_stopWatches.Contains(sw)) {
                _stopWatches.Remove(sw);
            }
        }
    }
    
    public class Timer
    {
        /// <summary>
        /// Has the timer finished running
        /// </summary>
        public bool HasFinished {
            get { return _elapsed >= duration; }
        }

        /// <summary>
        /// Is timer running right now.
        /// </summary>
        public bool Active { get; private set; }

        public float Elapsed {
            get { return _elapsed; }
        }

        private bool Invoke { get; set; }

        public float duration;

        private float _elapsed;
        private Action _onComplete;

        /// <summary>
        /// Create timer (autosubs and unsubs from manager, can be reset)
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="onComplete"></param>
        public Timer(float duration, bool autoStart, Action onComplete = null) {
            Invoke = true;
            this.duration = duration;

            _elapsed = 0f;
            _onComplete = onComplete;
            if (autoStart) {
                Start();
            }
        }

        ~Timer()
        {
            _onComplete = null;
        }

        public void Start()
        {
            TimerManager.Instance.AddTimer(this);
            Active = true;
        }
        
        public void Stop()
        {
            TimerManager.Instance.RemoveTimer(this);
            Active = false;
            Reset();
        }

        /// <summary>
        /// Reset and resubscribe timer
        /// </summary>
        public void Reset()
        {
            _elapsed = 0f;
        }

        public void Remove() {
            //prevents calling the oncomplete action
            Invoke = false;
            Active = false;
            //clear callback and set to finished, manager will remove
            _elapsed = duration + 1f;
        }

        /// <summary>
        /// Update timer, to check if it has finished. TimerManager handles deltaTime for timers.
        /// </summary>
        /// <param name="deltaTime">elapsed time since last update call.</param>
        /// <returns>True/false timer has finished.</returns>
        public bool Update(float deltaTime = 0f)
        {
            _elapsed += deltaTime;
            if (HasFinished) {
                Active = false;
                if (Invoke) {
                    _onComplete?.Invoke();
                }
                else {
                    Invoke = true;
                }
                
                return true;
            }

            return false;
        }
    }

    public class StopWatch
    {
        /// <summary>
        /// Time the stopwatch has been running for
        /// </summary>
        public float Elapsed { get; private set; }

        /// <summary>
        /// Create new stopwatch
        /// </summary>
        /// <param name="autoStart">should the stopwatch start running directly, or is start called later?</param>
        public StopWatch(bool autoStart)
        {
            Reset();
            
            if (autoStart)
                Start();
        }

        /// <summary>
        /// Set elapsed time to zero (does not start of stop the stopwatch).
        /// </summary>
        public void Reset()
        {
            Elapsed = 0f;
        }

        /// <summary>
        /// Start tracking time
        /// </summary>
        public void Start()
        {
            TimerManager.Instance.AddStopWatch(this);
        }

        /// <summary>
        /// Stop tracking time (does not reset, so can be used for pauses as well)
        /// </summary>
        public void Stop()
        {
            TimerManager.Instance.RemoveStopWatch(this);
        }

        /// <summary>
        /// internal for TimerManager, updates elapsed time, do not call yourself
        /// </summary>
        public void Update(float deltaTime = 0f)
        {
            Elapsed += deltaTime;
        }
    }
}