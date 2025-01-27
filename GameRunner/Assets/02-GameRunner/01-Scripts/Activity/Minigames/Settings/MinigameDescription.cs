using System;
using UnityEngine;
using UnityEngine.Events;

using Cohort.CustomAttributes;

namespace Cohort.GameRunner.Minigames {
	[Serializable]
	public class MinigameDescription {
		public State MinigameState {
			get { return _state; }
		}

		public string actionDescription = "Do minigame";
		public string sceneName = "";
		public string data;
		public int[] locations;
		public bool networked;
		[Tooltip("If timelimit is negative, the timer is disabled")]
		public float timeLimit = -1;

		[Tooltip("Events is called when the learning is finished while the user is in the activity.")]
		public UnityEvent onFinCinematic;

		[Tooltip("Event is when the learning is finished before the user was in the room.")]
		public UnityEvent onFinDirect;

		public UnityEvent onReset;

		[ReadOnly, SerializeField] private State _state = State.Open;
		[ReadOnly] public int index;
		[ReadOnly] public MingameLogEntry log;

		/// <summary>
		/// Sets the state of the learning.
		/// </summary>
		/// <param name="newState">New state of the learning.</param>
		/// <param name="init">Is this an initial/join room set data call?</param>
		public void SetState(State newState, bool init, bool forceInvoke = false) {
			if (forceInvoke || newState == State.Completed || newState == State.Failed) {
				if (init) {
					onFinDirect?.Invoke();
				}
				else {
					onFinCinematic?.Invoke();
				}
			}
			
			_state = newState;
		}

		public void Reset() {
			_state = State.Open;

			onReset?.Invoke();
		}

		public enum State {
			Open = 0,
			Active,

			//Available,
			Completed,
			Failed
		}
	}
}