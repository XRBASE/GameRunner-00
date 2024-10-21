namespace Cohort.Networking {

	public class StorageRequest : TokenWebRequest {
		private const string PREFIX = "storage";
		
		public StorageRequest(Method method, string postfix, string json = "") : base(method, "api/") {
			if (string.IsNullOrEmpty(postfix)) {
				_url += PREFIX;
			}
			else {
				_url += $"{PREFIX}/{postfix}";
			}

			if (!string.IsNullOrEmpty(json)) {
				_data = json;
			}
		}

#region Get
		/// <summary>
		/// Retrieve single data entry
		/// </summary>
		/// <param name="storageGuid">unique identifier for group of items.</param>
		public static StorageRequest GetStorageDataRequest(string storageGuid) {
			return new StorageRequest(Method.Get, $"{storageGuid}");
		}
		
		/// <summary>
		/// Retrieves space based data list.
		/// </summary>
		/// <param name="spaceId">unique identifier for space.</param>
		/// <param name="identifier">unique identifier for group of items.</param>
		public static StorageRequest GetSpaceDataRequest(string spaceId, string identifier = "") {
			StorageRequest req = new StorageRequest(Method.Get, $"space/{spaceId}");
			if (!string.IsNullOrEmpty(identifier)) {
				req.AddParameter("identifier", identifier);
			}
			
			return req;
		}
		
		/// <summary>
		/// Retrieves environment based data list.
		/// </summary>
		/// <param name="environmentId">unique identifier for environment.</param>
		/// <param name="identifier">unique identifier for group of items.</param>
		public static StorageRequest GetEnvironmentDataRequest(string environmentId, string identifier = "") {
			StorageRequest req = new StorageRequest(Method.Get, $"environment/{environmentId}");
			if (!string.IsNullOrEmpty(identifier)) {
				req.AddParameter("identifier", identifier);
			}
			
			return req;
		}
		
		/// <summary>
		/// Retrieves user's own storage list.
		/// </summary>
		/// <param name="identifier">unique identifier for group of items.</param>
		public static StorageRequest GetUserDataRequest(string identifier) {
			StorageRequest req = new StorageRequest(Method.Get, $"me");
			if (!string.IsNullOrEmpty(identifier)) {
				req.AddParameter("identifier", identifier);
			}
			
			return req;
		}
#endregion
#region Set
		/// <summary>
		/// Change data on the storage server, using the storage id.
		/// </summary>
		/// <param name="storageGuid">unique identifier for items.</param>
		/// <param name="json">new json body for the item.</param>
		public static StorageRequest UpdateDataRequest(string storageGuid, string json) {
			return new StorageRequest(Method.PostJSON, $"{storageGuid}", json);
		}
		
		/// <summary>
		/// Add space data entry to the storage.
		/// </summary>
		/// <param name="spaceId">unique identifier for space.</param>
		/// <param name="identifier">unique identifier for group of items.</param>
		/// <param name="json">json body for the item.</param>
		public static StorageRequest AddSpaceDataRequest(string spaceId, string json, string identifier = "") {
			StorageRequest req = new StorageRequest(Method.PostJSON, $"space/{spaceId}", json);
			if (!string.IsNullOrEmpty(identifier)) {
				req.AddParameter("identifier", identifier);
			}
			
			return req;
		}
		
		/// <summary>
		/// Add environment data entry to the storage.
		/// </summary>
		/// <param name="environmentId">unique identifier for environment.</param>
		/// <param name="json">json body for the item.</param>
		/// <param name="identifier">unique identifier for group of items.</param>
		public static StorageRequest AddEnvironmentDataRequest(string environmentId, string json, string identifier = "") {
			StorageRequest req = new StorageRequest(Method.PostJSON, $"environment/{environmentId}", json);
			if (!string.IsNullOrEmpty(identifier)) {
				req.AddParameter("identifier", identifier);
			}
			
			return req;
		}
#endregion

#region Delete
		/// <summary>
		/// Delete data in the storage.
		/// </summary>
		/// <param name="storageGuid">unique identifier for item.</param>
		public static StorageRequest DeleteDataRequest(string storageGuid) {
			return new StorageRequest(Method.Delete, $"{storageGuid}");
		}
#endregion
	}
}