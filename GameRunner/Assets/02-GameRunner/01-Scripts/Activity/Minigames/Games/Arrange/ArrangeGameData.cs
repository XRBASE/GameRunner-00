using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArrangeGameData
{
    public string title;
    public List<ArrangeData> ArrangeData;
    [HideInInspector]
    public List<string> chosenIds;
}