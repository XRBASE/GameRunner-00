using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEditor;

public static class ClearRoomProperties
{
    //should portals be deleted along with the photon room data?
    public const bool DELETE_PORTALS = true;
    
    [MenuItem("Cohort/ClearRoomProperties")]
    public static void ClearRoomPropsEditor()
    {
        ClearRoomProps();
    }
    
    /// <summary>
    /// Clear all property values in the current photon room
    /// </summary>
    public static void ClearRoomProps()
    {
        if (!Application.isPlaying || Network.Local?.Client?.CurrentRoom == null) {
            Debug.LogWarning("Cannot clear properties when not connected or in room");
            return;
        }

        Hashtable props = Network.Local.Client.CurrentRoom.CustomProperties;
        //clear old data
        List<object> keys = props.Keys.ToList();
        foreach (var key in keys) {
            //set all properties to null (this should clear them out)
            props[key] = null;
        }

        Network.Local.Client.CurrentRoom.SetCustomProperties(props);
        Debug.Log("Room properties cleared");
    }
}
