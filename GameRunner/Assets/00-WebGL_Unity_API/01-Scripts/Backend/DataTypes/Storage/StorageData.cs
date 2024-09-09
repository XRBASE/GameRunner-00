using System;
using UnityEngine.Serialization;

namespace Cohort.Ravel.BackendData.Data {
	
	[Serializable]
	public class StorageData : DataContainer {
		public override string Key {
			get { return id; }
		}

		public StorageData(string json) {
			jsonData = json;
		}

		public StorageData(string id, string json) {
			this.id = id;
			jsonData = json;
		}

		public string id;
		
		public string userEntityId;
		public string spaceEntityId;
		public string environmentEntityId;
		public string identifier;
		
		public string jsonData;

		public override bool Overwrite(DataContainer data) {
			if (data.GetType().IsSubclassOf(typeof(StorageData))) {
				StorageData storage = (StorageData)data;
				bool hasChanges = false;

				if (!string.IsNullOrEmpty(storage.jsonData)) {
					jsonData = storage.jsonData;
					hasChanges = true;
				}

				return hasChanges;
			}

			throw GetOverwriteFailedException(data);
		}
	}
}