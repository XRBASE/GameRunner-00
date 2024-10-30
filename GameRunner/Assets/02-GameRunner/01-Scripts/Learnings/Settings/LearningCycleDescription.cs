using System;
using UnityEngine;

[Serializable, CreateAssetMenu(menuName = "Cohort/Gamerunner/LearningCycle", fileName = "LearningCycleDescription")]
public class LearningCycleDescription : ScriptableObject {
    public LearningDescription[] learnings;
    public bool useTimer;
    public bool linear;
    public bool complete;
    public bool networked;

    public void OnValidate() {
        for (int i = 0; i < learnings.Length; i++) {
            learnings[i].index = i;
        }
    }
}
