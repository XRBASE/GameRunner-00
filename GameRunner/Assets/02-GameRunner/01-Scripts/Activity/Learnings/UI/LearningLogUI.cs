using System;
using Cohort.Patterns;
using Cohort.UI.Generic;
using UnityEngine;

public class LearningLogUI : UIPanel {
    [SerializeField] private LearningLogEntry _template;

    private ObjectPool<string, LearningLogEntry> _pool;
    
    private void Awake() {
        UILocator.Register(this);
        
        _pool = new ObjectPool<string, LearningLogEntry>(_template);
    }

    public void ClearLog() {
        _pool.SetAll(Array.Empty<string>());
    }

    public LearningLogEntry CreateLogEntry(string action, string location) {
        LearningLogEntry entry = _pool.AddItem(action + " " + location);
        
        return entry;
    }
}
