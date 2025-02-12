using System;
using UnityEngine;

[Serializable]
public class ActivityDescription {
    public static ActivityDescription EMPTY {
        get { return new ActivityDescription(); }
    }

    public string SceneName {
        get { return _sceneName; }
    }

    public bool IsEmpty {
        get { return string.IsNullOrEmpty(_sceneName); }
    }
    
    [SerializeField] private string _sceneName;//either scene name, or bundle url

    public ActivityDescription(string sceneName = "") {
        _sceneName = sceneName;
    }
}
