using System;
using UnityEngine;

/// <summary>
/// Minigame and or learning base class, used for connecting to the manager classes when game is initialized.
/// </summary>
public abstract class Learning : MonoBehaviour {
	/// <summary>
	/// Initializes the learning with given json data.
	/// </summary>
	/// <param name="gameData">Json data.</param>
	/// <param name="scoreMultiplier">Score multiplier, only used for displaying score.</param>
	/// <param name="onLearningFinished">Range(0,1) decimal percentage of completeness.</param>
	public abstract void Initialize(string gameData, int scoreMultiplier, Action<float> onLearningFinished);

	protected virtual void Awake() {
		//LearningManager.Instance.InitializeLearning(this);
	}
}
