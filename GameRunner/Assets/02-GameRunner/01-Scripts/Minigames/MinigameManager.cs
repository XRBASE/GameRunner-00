using Cohort.Patterns;
using UnityEngine;

public class MinigameManager : Singleton<MinigameManager> {
    [SerializeField] private MiniGameDescription[] _minigames;
}
