using System;
using System.Collections;
using MathBuddy.FloatExtentions;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
	/// <summary>
	/// Minigame base class, used for connecting to the manager classes when game is initialized.
	/// </summary>
	public abstract class Minigame : MonoBehaviour {

		protected abstract float CorrectVisualDuration { get; }
		protected abstract float FaultiveVisualDuration { get; }
		protected abstract float FinishedVisualDuration { get; }

		public abstract int Score {
			get;
			set;
		}

		public int MaxScore {
			get { return _scoreRange.max; }
		}
		
		public int MinScore {
			get { return _scoreRange.min; }
		}
		
		protected Action<FinishCause, int> _onFinished;
		protected Action _onExit;

		[SerializeField] protected IntRange _scoreRange;
		
		[SerializeField] private GameTimer _timer;
		
		
		/// <summary>
		/// Initializes the minigame with given json data.
		/// </summary>
		/// <param name="gameData">Json data.</param>
		/// <param name="onFinished">Range(0,1) decimal percentage of completeness.</param>
		public virtual void Initialize(string gameData, float timeLimit, int minScore, int maxScore, Action<FinishCause, int> onFinished, Action onExit) {
			_onFinished = onFinished;
			_onExit = onExit;
			_scoreRange = new IntRange(minScore, maxScore);

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
			float scorePercent = _scoreRange.GetTime(Score, true);
			FinishCause cause;
			
			if (scorePercent <= MinigameManager.FAILURE_THRESHOLD) {
				cause = FinishCause.FinFailed;
			} else if (scorePercent <= MinigameManager.AMAZING_THRESHOLD) {
				cause = FinishCause.FinSuccess;
			}
			else {
				cause = FinishCause.FinPerfect;
			}
			
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
			FinSuccess = 1<<0,
			FinFailed = 1<<1,
			FinPerfect = 1<<2,
			Timeout = 1<<3,
			ActivityStop = 1<<4
		}
	}
}