using Cohort.GameRunner.LocoMovement;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class WalkOnTrack : MonoBehaviour
{
    public int Count {
        get { return trackTargets.Count; }
    }

    public Vector3 this[int i] {
        get { return trackTargets[i].position; }
    }

    public UnityEvent onTrackStarted, onTrackEnded;
    public bool finishTrackOnStartReached;
    public bool autoWalk;
    public bool run;
    public List<Transform> trackTargets;
    
    /// <summary>
    /// Sets the player on a track with given positions
    /// </summary>
    public void StartTrack()
    {
        Locomotion.Local.FollowTrack(this, false);
    }

    public void StartTrackReversed()
    {
        Locomotion.Local.FollowTrack(this, true);
    }
}