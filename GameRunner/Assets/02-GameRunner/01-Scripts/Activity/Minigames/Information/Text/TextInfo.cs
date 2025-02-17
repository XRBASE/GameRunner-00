using System;
using UnityEngine;

/// <summary>
/// This object contains all the (customizable) information needed to show a piece of text to the player.
/// Used by the TextViewer minigame.
/// </summary>
[Serializable]
public class TextInfo {
    public Page[] pages;
    
    [Serializable]
    public struct Page {
        public string title;
        [TextArea] public string body;
    }
}
