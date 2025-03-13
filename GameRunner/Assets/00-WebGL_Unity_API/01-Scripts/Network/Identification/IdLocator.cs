#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Used to set all id's for everything inheriting UniqueId whenever the scene is saved.
/// </summary>
public class IdLocator : AssetModificationProcessor {
	
	/// <summary>
	/// Check if an id has already been taken by an object.
	/// </summary>
	/// <param name="item">Item containing the new Id that needs to be checked.</param>
	/// <returns>True/False id of item has already been used in all other items.</returns>
	public static bool IdTaken(UniqueId item) {
		UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).ToArray();
		for (int i = 0; i < holders.Length; i++) {
			if (holders[i] != item && holders[i].Identifier == item.Identifier) {
				if (EditorUtility.IsDirty(holders[i])) {
					Debug.LogWarning($"Double index ({item.Identifier}) detected for item {holders[i].name} and {item.name}. Index of {item.name} has been cleared!");
				}
				
				return true;
			}
		}

		return false;
	}
	
	/// <summary>
	/// Clears all the Id's of the currently open scene.
	/// Warning: this might break connections with minigames, where id's have already been set.
	/// </summary>
	[MenuItem("Cohort/ClearIds")]
	public static void ClearIds()
	{
		UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).ToArray();
		for (int i = 0; i < holders.Length; i++) {
			if (holders[i].Identifier == -1)
				continue;
			
			holders[i].Identifier = -1;
			EditorUtility.SetDirty(holders[i]);
		}
	}
	
	/// <summary>
	/// Sets all currently unset id's
	/// </summary>
	public static void SetIds() {
	    UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).OrderBy(i => i.Name).ToArray();
	    bool[] taken = new bool[holders.Length];
	    
	    // Object's Ids are only refreshed when the value is -1 or all id's are reset, so any indexes set in the editor
	    // do not have to be replaced.
	    
	    for (int i = 0; i < holders.Length; i++) {
		    if (holders[i].Identifier >= 0) {
			    taken[holders[i].Identifier] = true;
		    }
	    }

	    for (int i = 0; i < taken.Length; i++) {
		    if (taken[i])
			    continue;
		    
		    for (int j = 0; j < holders.Length; j++) {
			    if (holders[j].Identifier >= 0) {
				    continue;
			    }

			    holders[j].Identifier = i;
			    EditorUtility.SetDirty(holders[j]);
			    break;
		    }
	    }
    }
	
	/// <summary>
	/// Triggers the id setting process whenever the scene in the project is saved.
	/// </summary>
    public static string[] OnWillSaveAssets(string[] paths) {
	    SetIds();
	    
	    return paths;
    }
}
#endif