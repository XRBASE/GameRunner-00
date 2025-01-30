using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

[Serializable]
public class MatchGameData
{
    public string title;
    public List<MatchPairData> matchPairs;
    [HideInInspector]
    public List<string> chosenIds;
}


