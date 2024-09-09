using System;
using UnityEngine;

namespace Cohort.Ravel.BackendData.Data {
	public class StorageService : MonoBehaviour {
		private StorageRepository _repo;

		private void Awake() {
			_repo = new StorageRepository();
		}
		
		/// <summary>
		/// Get storage item, using item-identifier.
		/// </summary>
		/// <param name="itemId">Unique identifier for item.</param>
		/// <param name="onComplete">Data retrieved callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void GetStorageData(string itemId, Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			StartCoroutine(_repo.RetrieveStorageData(itemId, onComplete, onFailure));
		}
		
		/// <summary>
		/// Change storage item, using item-identifier.
		/// </summary>
		/// <param name="itemId">Unique identifier for item.</param>
		/// <param name="json">New item body data.</param>
		/// <param name="onComplete">Data changed callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void ChangeStorageData(string itemId, string json, Action onComplete = null, Action<string> onFailure = null) {
			StartCoroutine(_repo.UpdateStorageItem(itemId, json, onComplete, onFailure));
		}
		
		/// <summary>
		/// Adds or changes a storage item based on the input storage data. If the input data has an identifier, the item with that
		/// identifier is changed, otherwise it will either add a space entry (if the space id of the data is set) or an environment
		/// entry if the environment id is set.
		/// </summary>
		/// <param name="data">Storage data of the item, set id to access existing item, set environment- or spaceId to create an item.</param>
		/// <param name="onComplete">Callback for when item has been changed or created with new, updated storage data that contains the items id.</param>
		/// <param name="onFailure">Failure callback for when item has not been created or changed.</param>
		public void SetStorageData(StorageData data, Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			StartCoroutine(_repo.SetStorageData(data, onComplete, onFailure));
		}
		
		/// <summary>
		/// Delete storage item, using item-identifier.
		/// </summary>
		/// <param name="itemId">Unique identifier for item.</param>
		/// <param name="onComplete">Data deleted callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void DeleteStorageData(string itemId, Action onComplete = null, Action<string> onFailure = null) {
			StartCoroutine(_repo.DeleteStorageItem(itemId, onComplete, onFailure));
		}
		
		/// <summary>
		/// Get space data, using group identifier.
		/// </summary>
		/// <param name="spaceId">Unique identifier of space.</param>
		/// <param name="identifier">Sub- (group) identifier.</param>
		/// <param name="onComplete">Data retrieved callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void GetSpaceStorageData(string spaceId, string identifier = "", 
		                                Action<StorageData[]> onComplete = null, Action<string> onFailure = null) {
			StartCoroutine(_repo.RetrieveSpaceStorageData(spaceId, identifier, onComplete, onFailure));
		}
		
		/// <summary>
		/// Add space item to storage.
		/// </summary>
		/// <param name="spaceId">Unique identifier of space.</param>
		/// <param name="identifier">Sub- (group) identifier.</param>
		/// <param name="onComplete">Data set callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void AddSpaceStorageData(string spaceId, string json, string identifier = "", 
		                                Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			
			StartCoroutine(_repo.AddSpaceStorageItem(spaceId, json, identifier, onComplete, onFailure));
		}
		
		/// <summary>
		/// Get environment data, using group identifier.
		/// </summary>
		/// <param name="environmentId">Unique identifier of environment.</param>
		/// <param name="identifier">Sub- (group) identifier.</param>
		/// <param name="onComplete">Data retrieved callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void GetEnvironmentStorageData(string environmentId, string identifier = "", 
		                                      Action<StorageData[]> onComplete = null, Action<string> onFailure = null) {
			
			StartCoroutine(_repo.RetrieveEnvironmentStorageData(environmentId, identifier, onComplete, onFailure));
		}
		
		/// <summary>
		/// Add environment data, using group identifier.
		/// </summary>
		/// <param name="environmentId">Unique identifier of environment.</param>
		/// <param name="identifier">Sub- (group) identifier.</param>
		/// <param name="json">Json data for the body of the item.</param>
		/// <param name="onComplete">Data set callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void AddEnvironmentData(string environmentId, string json, string identifier = "", 
		                               Action<StorageData> onComplete = null, Action<string> onFailure = null) {
			
			StartCoroutine(_repo.AddEnvironmentStorageItem(environmentId, json, identifier, onComplete, onFailure));
		}
		
		/// <summary>
		/// Retrieve all user's storage data.
		/// </summary>
		/// <param name="identifier">Sub- (group) identifier.</param>
		/// <param name="onComplete">Data retrieved callback.</param>
		/// <param name="onFailure">Failure error callback.</param>
		public void GetUserStorageData(string identifier, Action<StorageData[]> onComplete, Action<string> onFailure) {
			StartCoroutine(_repo.RetrieveUserStorageData(identifier, onComplete, onFailure));
		}

		public string GetIdentifier(StorageDataType type) {
			return type.ToString();
		}
		
		/// <summary>
		/// Unique keys for storage data identifiers.
		/// Add new identifiers to google sheets: https://docs.google.com/spreadsheets/d/1FhNdohaAcMJwWR2BZ_Pd6ettCdQ3lkXTFYVsIe8bPlc/edit?gid=1003720486#gid=1003720486
		/// </summary>
		public enum StorageDataType {
			None = 0,
			Assessments = 1,
		}
	}
}