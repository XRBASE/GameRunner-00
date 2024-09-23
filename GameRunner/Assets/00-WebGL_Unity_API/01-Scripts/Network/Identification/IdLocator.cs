#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

public class IdLocator : AssetModificationProcessor {
    public static void SetIds() {
	    IUniqueId[] holders = GameObject.FindObjectsOfType<MonoBehaviour>(true).OfType<IUniqueId>().ToArray();

	    for (int i = 0; i < holders.Length; i++) {
		    holders[i].Identifier = i + 1;
		    
		    EditorUtility.SetDirty(holders[i] as MonoBehaviour);
	    }
    }

    public static string[] OnWillSaveAssets(string[] paths) {
	    SetIds();
	    
	    return paths;
    }
}
#endif