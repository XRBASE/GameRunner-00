using System;

namespace Cohort.GameRunner.Avatars {
	[Serializable]
	public class RPMImportData : ImportData {
		public override AvatarType Type {
			get { return AvatarType.ReadyPlayerMe; }
		}

		public string url;

		/// <summary>
		/// Ready player me import description.
		/// </summary>
		/// <param name="url">web url at which model is saved</param>
		/// <param name="guid">unique identifier that identifies model within ravel.</param>
		/// <param name="gender">Gender 0 for feminine, 1 for masculine. Non-specified gender will use outfit to determine gender.</param>
		public RPMImportData(string url, string guid, float gender = -1f) : base(guid, gender) {
			this.url = url;
		}
	}
}