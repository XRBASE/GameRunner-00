using System;
using UnityEngine;

public abstract class Minigame : MonoBehaviour {
    public abstract void Initialize(string gameData, Action<bool> onGameFinished);

    protected void Awake() {
        MinigameManager.Instance.SetupMinigame(this);
    }
}
