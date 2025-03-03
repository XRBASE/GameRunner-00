using System;
using UnityEngine;

/// <summary>
/// This object contains all the (customizable) information needed to show a video to the player.
/// Used by the VideoViewer minigame.
/// </summary>
[Serializable]
public class VideoInfo
{
	[Tooltip("This is the backend uuid, which points to the video asset.")]
	public string uuid;
}
