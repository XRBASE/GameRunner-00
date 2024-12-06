using System;
using UnityEngine;

[Serializable]
public class ActivityDescription {
    public static ActivityDescription EMPTY {
        get { return new ActivityDescription(); }
    }
    
    public bool IsBundle {
        get { return _isBundle; }
    }

    public string AssetRef {
        get { return _assetRef; }
    }

    public int ScoreMultiplier {
        get { return _scoreMultiplier; }
    }

    public bool IsEmpty {
        get { return string.IsNullOrEmpty(_assetRef); }
    }

    [SerializeField] private bool _isBundle = false;
    [SerializeField] private string _assetRef;//either scene name, or bundle url
    [SerializeField] private int _scoreMultiplier = 1;

    public ActivityDescription(bool isBundle = false, string assetRef = "", int scoreMultiplier = 1) {
        _isBundle = isBundle;
        _assetRef = assetRef;
        _scoreMultiplier = scoreMultiplier;
    }
}
