using System;

namespace Cohort.GameRunner.Minigames {
    [Serializable]
    public class MinigameCycleDescription {
        public MinigameDescription[] minigames;
        public bool useTimer;
        public bool linear;
        public bool complete;
        public bool networked;

        public void OnValidate() {
            if (minigames == null)
                return;
            
            for (int i = 0; i < minigames.Length; i++) {
                minigames[i].index = i;
            }
        }
    }
}