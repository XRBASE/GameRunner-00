using System;
using System.Collections;
using Cohort.Ravel.Networking;

namespace Cohort.Ravel.BackendData.Data {

	public class StorageRepository : DataRepository<StorageData> {
		public StorageRepository() : base(false, new TimeSpan(0)) { }

		public IEnumerator RetrieveStorageData(string id, Action<StorageData> onComplete, Action<string> onFailure) {
			StorageData data;
			if (TryGetFromCache(id, out data)) {
				onComplete?.Invoke(data);
				yield break;
			}
			
			RavelWebRequest req = StorageRequest.GetStorageDataRequest(id);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.TryGetData(out data)) {
				onComplete?.Invoke(data);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		
		public IEnumerator UpdateStorageItem(string id, string json, Action onComplete, Action<string> onFailure) {
			RavelWebRequest req = StorageRequest.UpdateDataRequest(id, json);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success) {
				onComplete?.Invoke();
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}

		public IEnumerator DeleteStorageItem(string id, Action onComplete, Action<string> onFailure) {
			RavelWebRequest req = StorageRequest.DeleteDataRequest(id);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success) {
				onComplete?.Invoke();
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		
		public IEnumerator RetrieveSpaceStorageData(string spaceId, string identifier = "", Action<StorageData[]> onComplete = null, Action<string> onFailure = null) {
			ProxyCollection<StorageData> data;
			//no cache yet
			
			RavelWebRequest req = StorageRequest.GetSpaceDataRequest(spaceId, identifier);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.TryGetCollection(out data)) {
				onComplete?.Invoke(data.Array);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}

		public IEnumerator AddSpaceStorageItem(string spaceId, string json, string identifier = "", 
		                                       Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			RavelWebRequest req = StorageRequest.AddSpaceDataRequest(spaceId, json, identifier);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success) {
				StorageData data = new StorageData(res.DataString, json);
				
				CacheData(data);
				onComplete?.Invoke(data);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		
		public IEnumerator RetrieveEnvironmentStorageData(string envId, string identifier = "", 
		                                                  Action<StorageData[]> onComplete = null, Action<string> onFailure = null) {
			ProxyCollection<StorageData> data;
			//no cache yet
			
			RavelWebRequest req = StorageRequest.GetEnvironmentDataRequest(envId, identifier);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.TryGetCollection(out data)) {
				onComplete?.Invoke(data.Array);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}

		public IEnumerator AddEnvironmentStorageItem(string envId, string json, string identifier = "", 
		                                             Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			RavelWebRequest req = StorageRequest.AddEnvironmentDataRequest(envId, json, identifier);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success) {
				StorageData data = new StorageData(res.DataString, json);
				
				CacheData(data);
				onComplete?.Invoke(data);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		
		public IEnumerator RetrieveUserStorageData(string identifier, Action<StorageData[]> onComplete, Action<string> onFailure) {
			ProxyCollection<StorageData> data;
			//no cache yet
			
			RavelWebRequest req = StorageRequest.GetUserDataRequest(identifier);

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.TryGetCollection(out data)) {
				onComplete?.Invoke(data.Array);
			}
			else {
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		
		public IEnumerator SetStorageData(StorageData data, Action<StorageData> onComplete, Action<string> onFailure) {
			RavelWebRequest req;
			if (string.IsNullOrEmpty(data.id)) {
				if (!string.IsNullOrEmpty(data.spaceEntityId)) {
					req = StorageRequest.AddSpaceDataRequest(data.spaceEntityId, data.jsonData, data.identifier);
				} else if (!string.IsNullOrEmpty(data.environmentEntityId)) {
					req = StorageRequest.AddEnvironmentDataRequest(data.environmentEntityId, data.jsonData, data.identifier);
				}
				else {
					onFailure?.Invoke("Missing id exception, space or environment id is required to add an item.");
					yield break;
				}
			}
			else {
				req = StorageRequest.UpdateDataRequest(data.Key, data.jsonData);
			}

			yield return req.Send();
			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.TryGetData(out string guid)) {
				data.id = guid;
				
				onComplete?.Invoke(data);
			} else
			{
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
	}
}