using System;
using UnityEngine;
using UnityEngine.Events;

using Cohort.CustomAttributes;

namespace Cohort.GameRunner.Minigames {
	[Serializable]
	public class MinigameDescription {
		public State state;

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
		
		[ReadOnly] public int index;
		[ReadOnly] public MingameLogEntry log;
		

		public void SetState(State newState, bool init) {
			SetStatus(newState.status, init);
			state = newState;
		}

		/// <summary>
		/// Sets the status of the minigame.
		/// </summary>
		/// <param name="newState">New state of the learning.</param>
		/// <param name="init">Is this an initial/join room set data call?</param>
		/// <param name="forceInvoke">Force invoke of the matching event?</param>
		public void SetStatus(Status newState, bool init, bool forceInvoke = false) {
			if (forceInvoke || newState == Status.Completed || newState == Status.Failed) {
				if (init) {
					onFinDirect?.Invoke();
				}
				else {
					onFinCinematic?.Invoke();
				}
			}
			state.status = newState;
		}

		public void Reset() {
			state.status = Status.Open;

			onReset?.Invoke();
		}

		public enum Status {
			Open = 0,
			Available,
			Active,
			Completed,
			Failed
		}

		[Serializable]
		public struct State {
			[ReadOnly] public Status status;
			[ReadOnly] public int location;
		}
	}
}