using System;
using Cohort.Patterns;
using Cohort.UI.Generic;
using UnityEngine;

public class MinigameUILog : UIPanel {
    [SerializeField] private MinigameLogEntry _templateEntry;
    
    private ObjectPool<string, MinigameLogEntry> _pool;

    private void Awake() {
        UILocator.Register(this);
        
        _pool = new ObjectPool<string, MinigameLogEntry>(_templateEntry);
    }

    public MinigameLogEntry AddEntry(MinigameInteractable interactable, MiniGameDescription game) {
        return _pool.AddItem(FormatString(game.actionDescription, interactable.Description));
    }

    public void RemoveEntry(MinigameLogEntry entry) {
        _pool.RemoveItem(entry);
    }

    public void Clear() {
        _pool.SetAll(Array.Empty<string>());
    }

    private string FormatString(string actionString, string locationString) {
        return actionString + " " + locationString + "!";
    }
}
