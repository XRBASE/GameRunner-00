using System;
using UnityEngine;

[Serializable]
public class TextInfo {
    public Page[] pages;
    
    [Serializable]
    public struct Page {
        public string title;
        [TextArea] public string body;
    }
}
