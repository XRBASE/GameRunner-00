#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class IdLocator : AssetModificationProcessor {
	[MenuItem("Cohort/ClearIds")]
	public static void ClearIds()
	{
		UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).ToArray();
		for (int i = 0; i < holders.Length; i++) {
			if (!holders[i].IdentifierSet)
				continue;
			
			holders[i].Identifier = -1;
			EditorUtility.SetDirty(holders[i]);
		}
	}
	
	public static void SetIds() {
	    UniqueId[] holders = Object.FindObjectsOfType<UniqueId>(true).OrderBy(i => i.Name).ToArray();
	    bool[] taken = new bool[holders.Length];

	    for (int i = 0; i < holders.Length; i++) {
		    if (holders[i].Identifier >= 0 && holders[i].IdentifierSet) {
			    taken[holders[i].Identifier] = true;
		    }
		    else {
			    holders[i].Identifier = -1;
		    }
	    }

	    for (int i = 0; i < taken.Length; i++) {
		    if (taken[i])
			    continue;
		    
		    for (int j = 0; j < holders.Length; j++) {
			    if (holders[j].IdentifierSet) {
				    continue;
			    }

			    holders[j].Identifier = i;
			    EditorUtility.SetDirty(holders[i]);
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