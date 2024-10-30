using Cohort.UI.Generic;
using UnityEngine;

public class LearningLogUI : UIPanel {
    [SerializeField] private Transform _logParent;
    [SerializeField] private LearningLogEntry _template;
    
    private void Awake() {
        UILocator.Register(this);
        _template.gameObject.SetActive(false);
        
        //TODO_COHORT: objectpool
    }

    public LearningLogEntry CreateLogEntry(string action, string location) {
        LearningLogEntry entry = Instantiate(_template, _logParent);
        entry.Text = action + location + "!";
        
        entry.gameObject.SetActive(true);
        
        return entry;
    }
}
