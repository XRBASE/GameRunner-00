using System;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
	[Serializable]
	public class PrefabImportData : ImportData {
		private const string RESOURCE_PREFIX = "Avatars/Prefab/";

		public override AvatarType Type {
			get { return AvatarType.Prefab; }
		}

		//using scene or project asset
		public GameObject prefab;

		//should new instance be instantiated
		public bool preloaded;

		//use when loading from resource folder, leave empty otherwise
		public string resourcePath;

		/// <summary>
		/// Creates prefab data for prefabs loaded from within the resource folder.
		/// </summary>
		/// <param name="resourcePath">path of resource starting at the "Resources/Avatars/Prefab/ folder."</param>
		/// <param name="gender">Used for the gender selection of animations (0 = feminine, 1 = masculine)</param>
		public PrefabImportData(string resourcePath, string guid, float gender = 0.5f) : base(guid, gender) {
			this.resourcePath = resourcePath;
		}

		/// <summary>
		/// Creates prefab data for prefab files (in or outside of the scene).
		/// </summary>
		/// <param name="prefab">prefab object, either in the scene or project hierarchy."</param>
		/// <param name="preloaded">Is the prefab object in the scene (true) or project hierarchy (false)."</param>
		/// <param name="gender">Used for the gender selection of animations (0 = feminine, 1 = masculine)</param>
		public PrefabImportData(GameObject prefab, bool preloaded, string guid, float gender = 0.5f) : base(
			guid, gender) {
			this.prefab = prefab;
			this.preloaded = preloaded;
		}
	}
}