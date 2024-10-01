using System;
using UnityEngine;

[Serializable]
public class GameDefinition {
    public bool IsBundle {
        get { return _isBundle; }
    }

    public string AssetRef {
        get { return _assetRef; }
    }

    [SerializeField] private bool _isBundle = false;
    [SerializeField] private string _assetRef;//either scene name, or bundle url
}
