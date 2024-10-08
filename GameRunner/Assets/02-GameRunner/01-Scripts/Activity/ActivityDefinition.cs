using System;
using UnityEngine;

[Serializable]
public class ActivityDefinition {
    public bool IsBundle {
        get { return _isBundle; }
    }

    public string AssetRef {
        get { return _assetRef; }
    }

    public int ScoreMultiplier {
        get { return _scoreMultiplier; }
    }

    [SerializeField] private bool _isBundle = false;
    [SerializeField] private string _assetRef;//either scene name, or bundle url
    [SerializeField] private int _scoreMultiplier = 1;
}
