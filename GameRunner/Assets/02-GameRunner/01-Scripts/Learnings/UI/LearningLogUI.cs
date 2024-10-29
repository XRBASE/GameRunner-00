using Cohort.UI.Generic;
using UnityEngine;

public class LearningLogUI : UIPanel {
    [SerializeField] private Transform _logParent;
    [SerializeField] private LearningLogEntry _template;
    
    private void Awake() {
        UILocator.Register(this);
    }

    public LearningLogEntry CreateLogEntry(string action, string location) {
        LearningLogEntry entry = Instantiate(_template, _logParent);
        entry.Text = action + location + "!";
        
        return entry;
    }
}
