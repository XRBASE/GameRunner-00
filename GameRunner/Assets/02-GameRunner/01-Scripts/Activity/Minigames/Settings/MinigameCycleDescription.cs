using System;
using System.Collections.Generic;
using UnityEngine;

namespace Cohort.GameRunner.Minigames {
    [Serializable]
    public class MinigameCycleDescription {
        public MinigameDescription[] minigames;

        public string GetMinigameStateString() {
            MinigameDescription.State[] states = new MinigameDescription.State[minigames.Length];

            for (int i = 0; i < minigames.Length; i++) {
                states[i] = minigames[i].MinigameState;
            }
            //potentially faster if converted to integers
            string data = MathBuddy.Strings.StringExtentions.ToCsv(states);
            return data;
        }

        public void SetMinigameStates(string data) {
            List<string> states = MathBuddy.Strings.StringExtentions.FromCsv(data);

            for (int i = 0; i < states.Count; i++) {
                minigames[i].SetState(Enum.Parse<MinigameDescription.State>(states[i]), true);
            }
        }
        
        public void OnValidate() {
            if (minigames == null)
                return;
            
            for (int i = 0; i < minigames.Length; i++) {
                minigames[i].index = i;
            }
        }
    }
}