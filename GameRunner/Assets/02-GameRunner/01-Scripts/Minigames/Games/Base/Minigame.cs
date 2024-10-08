using System;
using UnityEngine;

public abstract class Minigame : MonoBehaviour {
    /// <summary>
    /// Initializes the minigame with given json data.
    /// </summary>
    /// <param name="gameData">json data.</param>
    /// <param name="onGameFinished">Range(0,1) decimal percentage of completeness.</param>
    public abstract void Initialize(string gameData, Action<float> onGameFinished);

    protected void Awake() {
        MinigameManager.Instance.SetupMinigame(this);
    }
}
