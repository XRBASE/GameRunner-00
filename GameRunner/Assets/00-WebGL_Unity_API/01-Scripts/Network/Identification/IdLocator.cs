#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class IdLocator : AssetModificationProcessor {
    public static void SetIds() {
	    IUniqueId[] holders = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IUniqueId>().ToArray();
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
			    EditorUtility.SetDirty(holders[i] as MonoBehaviour);
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