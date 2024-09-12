using System;

namespace Cohort.GameRunner.Avatars {
	[Serializable]
	public class GLBImportData : ImportData {
		public override AvatarType Type {
			get { return AvatarType.Glb; }
		}

		/// <summary>
		/// Ready player me import description.
		/// </summary>
		/// <param name="guid">unique identifier that identifies model within ravel.</param>
		/// <param name="gender">Gender 0 for feminine, 1 for masculine. Non-specified gender will use outfit to determine gender.</param>
		public GLBImportData(string guid, float gender = -1f) : base(guid, gender) { }
	}
}