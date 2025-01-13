using System;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
	/// <summary>
	/// Minigame base class, used for connecting to the manager classes when game is initialized.
	/// </summary>
	public abstract class Minigame : MonoBehaviour {

		public abstract float Score {
			get;
			set;
		}
		
		protected Action<float> _onFinished;
		protected Action _onExit;

		/// <summary>
		/// Initializes the minigame with given json data.
		/// </summary>
		/// <param name="gameData">Json data.</param>
		/// <param name="scoreMultiplier">Score multiplier, only used for displaying score.</param>
		/// <param name="onFinished">Range(0,1) decimal percentage of completeness.</param>
		public virtual void Initialize(string gameData, int scoreMultiplier, Action<float> onFinished, Action onExit) {
			_onFinished = onFinished;
			_onExit = onExit;
		}

		/// <summary>
		/// Finishes the minigame and assigns the currently earned score to the player.
		/// </summary>
		public virtual void FinishMinigame() {
			_onFinished?.Invoke(Score);
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
	}
}