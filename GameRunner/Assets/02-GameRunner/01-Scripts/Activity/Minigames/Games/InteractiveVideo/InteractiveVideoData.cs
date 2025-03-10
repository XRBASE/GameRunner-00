using System;
using System.Collections.Generic;

[Serializable]
public class InteractiveVideoData : VideoInfo
{
    public string titleText;
    public List<Popup> popups;
    public List<string> chosenIds;
}
