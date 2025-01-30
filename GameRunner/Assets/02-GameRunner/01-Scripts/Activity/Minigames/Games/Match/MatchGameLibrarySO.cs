using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Use this to fill all potentially used match pairs and serialise this into the MatchGame
///  After doing this you can use the MatchGameData to refer to the ids in this library for a chosen configuration
/// </summary>

[CreateAssetMenu(fileName = "MatchGameDataLibrarySo", menuName = "Cohort/MatchGameLibrary", order = 2)]
public class MatchGameLibrarySO : ScriptableObject
{
    public List<MatchPairData> MatchGameLibrary;
}
