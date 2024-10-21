using System;
using UnityEngine;

public abstract class Minigame : MonoBehaviour {
    public bool IsPlaying { get; protected set; }
    
    /// <summary>
    /// Initializes the minigame with given json data.
    /// </summary>
    /// <param name="gameData">json data.</param>
    /// <param name="onGameFinished">Range(0,1) decimal percentage of completeness.</param>
    public abstract void Initialize(string gameData, int scoreMultiplier, Action<float> onGameFinished);

    protected virtual void Awake() {
        MinigameManager.Instance.SetupMinigame(this);
    }
}
