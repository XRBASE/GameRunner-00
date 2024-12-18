using System;
using UnityEngine;

using Cohort.Patterns;
using Cohort.UI.Generic;

namespace Cohort.GameRunner.Minigames {
    public class MinigameLogUI : UIPanel {
        [SerializeField] private MingameLogEntry _template;

        private ObjectPool<string, MingameLogEntry> _pool;

        private void Awake() {
            UILocator.Register(this);

            _pool = new ObjectPool<string, MingameLogEntry>(_template);
        }

        public void ClearLog() {
            _pool.SetAll(Array.Empty<string>());
        }

        public MingameLogEntry CreateLogEntry(string action, string location) {
            MingameLogEntry entry = _pool.AddItem(action + " " + location);

            return entry;
        }
    }
}