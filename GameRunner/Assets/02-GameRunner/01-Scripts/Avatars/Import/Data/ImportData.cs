namespace Cohort.GameRunner.Avatars {
	public abstract class ImportData {
		public abstract AvatarType Type { get; }
		
		public string GUID { get; private set; }
		public float Gender { get; set; }
		
		public ImportData(string guid, float gender) {
			GUID = guid;
			Gender = gender;
		}
		
		public enum AvatarType {
			ReadyPlayerMe,
			Prefab,
			Glb
		}
	}
}