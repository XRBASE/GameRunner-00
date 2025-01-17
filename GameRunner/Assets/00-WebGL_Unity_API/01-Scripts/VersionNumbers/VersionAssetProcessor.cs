#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class VersionAssetProcessor : AssetPostprocessor {
	//written in uppercase to simplify search
	private const string VERSION_ASSET_NAME = "VERSIONLOG";
	private const string VERSION_ASSET_EXTENSION = ".MD";
	
	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
	                                           string[] movedFromAssetPaths) {
		for (int i = 0; i < importedAssets.Length; i++) {
			if (Path.GetExtension(importedAssets[i]).ToUpper() == VERSION_ASSET_EXTENSION &&
			    Path.GetFileNameWithoutExtension(importedAssets[i])?.ToUpper() == VERSION_ASSET_NAME) {

				SetVersionNumber(importedAssets[i]);
			}
		}
	}

	private static void SetVersionNumber(string asset) {
		string path = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + asset;
		string version = File.ReadAllLines(path)[0].Substring(2) + "_" + PlayerSettings.productName;
	    
		Debug.LogWarning($"Updating version number to {version}!");
		PlayerSettings.bundleVersion = version;
	}
}
#endif