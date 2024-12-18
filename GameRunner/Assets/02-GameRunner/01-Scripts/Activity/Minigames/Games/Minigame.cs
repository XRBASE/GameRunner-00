using System;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
	/// <summary>
	/// Minigame base class, used for connecting to the manager classes when game is initialized.
	/// </summary>
	public abstract class Minigame : MonoBehaviour {
		/// <summary>
		/// Initializes the minigame with given json data.
		/// </summary>
		/// <param name="gameData">Json data.</param>
		/// <param name="scoreMultiplier">Score multiplier, only used for displaying score.</param>
		/// <param name="onLearningFinished">Range(0,1) decimal percentage of completeness.</param>
		public abstract void Initialize(string gameData, int scoreMultiplier, Action<float> onLearningFinished);

		public abstract void StopMinigame();

		protected virtual void Awake() {
			MinigameManager.Instance.InitializeMinigame(this);
		}
	}
}