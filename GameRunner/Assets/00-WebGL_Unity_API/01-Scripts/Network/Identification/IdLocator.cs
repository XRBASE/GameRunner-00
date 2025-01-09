#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class IdLocator : AssetModificationProcessor {
	
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

	public static void SetIds() {
	    UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).OrderBy(i => i.Name).ToArray();
	    bool[] taken = new bool[holders.Length];

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

    public static string[] OnWillSaveAssets(string[] paths) {
	    SetIds();
	    
	    return paths;
    }
}
#endif