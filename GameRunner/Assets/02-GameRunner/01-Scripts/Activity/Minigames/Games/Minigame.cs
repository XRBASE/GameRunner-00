using System;
using System.Collections;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
	/// <summary>
	/// Minigame base class, used for connecting to the manager classes when game is initialized.
	/// </summary>
	public abstract class Minigame : MonoBehaviour {

		protected abstract float CorrectVisualDuration { get; }
		protected abstract float FaultiveVisualDuration { get; }
		protected abstract float FinishedVisualDuration { get; }

		public abstract float Score {
			get;
			set;
		}
		
		protected Action<FinishCause, float> _onFinished;
		protected Action _onExit;

		[SerializeField] private GameTimer _timer;
		

		/// <summary>
		/// Initializes the minigame with given json data.
		/// </summary>
		/// <param name="gameData">Json data.</param>
		/// <param name="scoreMultiplier">Score multiplier, only used for displaying score.</param>
		/// <param name="onFinished">Range(0,1) decimal percentage of completeness.</param>
		public virtual void Initialize(string gameData, float timeLimit, Action<FinishCause, float> onFinished, Action onExit) {
			_onFinished = onFinished;
			_onExit = onExit;

			if (timeLimit < 0) {
				_timer.gameObject.SetActive(false);
			}
			else {
				_timer.gameObject.SetActive(true);
				_timer.Initialize(timeLimit, TimeLimitReached);
			}
		}

		protected void TimeLimitReached() {
			FinishMinigame(FinishCause.Timeout);
		}

		public virtual void FinishMinigame() {
			FinishCause cause = Score <= MinigameManager.FAILURE_THRESHOLD ? FinishCause.Failed : FinishCause.Completed;
			
			FinishMinigame(cause);
		}

		/// <summary>
		/// Finishes the minigame and assigns the currently earned score to the player.
		/// </summary>
		public void FinishMinigame(FinishCause cause) {
			_onFinished?.Invoke(cause, Score);
		}

		/// <summary>
		/// Stops minigame without finishing it, it is still possible to restart the game afterwards and no score is earned.
		/// </summary>
		public virtual void ExitMinigame() {
			_onExit?.Invoke();
		}

		protected virtual void Awake() {
			MinigameManager.Instance.InitializeMinigame(this);
		}

		protected IEnumerator DoTimeout(float duration, Action onComplete) {
			yield return new WaitForSeconds(duration);
			
			onComplete?.Invoke();
		}
		
		[Flags]
		public enum FinishCause {
			None = 0,
			Completed = 1<<0,
			Failed = 1<<1,
			Timeout = 1<<2,
			ActivityStop = 1<<3
		}
	}
}