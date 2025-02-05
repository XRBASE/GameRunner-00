using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
    [Serializable]
    public class MinigameCycleDescription {
        public MinigameDescription[] minigames;
        
        public void OnValidate() {
            if (minigames == null)
                return;
            
            for (int i = 0; i < minigames.Length; i++) {
                minigames[i].index = i;
            }
        }
    }
}