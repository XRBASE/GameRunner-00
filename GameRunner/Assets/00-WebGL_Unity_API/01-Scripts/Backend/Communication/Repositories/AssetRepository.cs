using System;
using System.Collections;
using Cohort.Ravel.Assets;
using Cohort.Ravel.Networking;
using Cohort.Ravel.Networking.Spaces;
using UnityEngine;
using Space = Cohort.Ravel.Spaces.Space;

public class AssetRepository : DataRepository<Asset>
{
	public AssetRepository() : base(true, new TimeSpan(0, 0, 1, 0)) {
		
	}
	
	/// <summary>
	/// Retrieves all (personal)assets of the currently logged in user.
	/// </summary>
	/// <param name="onComplete">callback for when assets have been retrieved.</param>
	/// <param name="onFailure">callback for when something has gone wrong retrieving assets (passes error string).</param>
	public IEnumerator RetrieveUserAssets(Action<Asset[]> onComplete, Action<string> onFailure) {
		RavelWebRequest req = AssetRequest.GetPublicAssets();
		yield return req.Send();
		RavelWebResponse res = new RavelWebResponse(req);
		
		if (res.Success && ProxyCollection<Asset>.TryGet(res.DataString, out ProxyCollection<Asset> assets))
		{
			onComplete?.Invoke(assets.Array);
		}
		else {
			Debug.LogError(res.Error.FullMessage);
			onFailure?.Invoke(res.Error.FullMessage);
		}
	}
	
	/// <summary>
	/// Retrieves an asset's data.
	/// </summary>
	/// <param name="asset">Asset of which data needs to be retrieved.</param>
	/// <param name="onComplete">Complete callback, including asset.</param>
	/// <param name="onFailure">Failure callback, including error.</param>
	public IEnumerator RetrieveAssetData(Asset asset, Action<Asset> onComplete, Action<string> onFailure) {
		if (asset.HasData) {
			onComplete?.Invoke(asset);
			yield break;
		}

		if (string.IsNullOrEmpty(asset.id)) {
			Debug.LogError($"Could not download file without assetId: {asset}");
			onFailure?.Invoke($"Could not download file without assetId: {asset}");
		}

		Asset newAsset;
		if (!TryGetFromCache(asset, out newAsset)) {
			AssetRequest req = AssetRequest.DownloadAsset(asset.id);
			yield return req.Send();

			RavelWebResponse res = new RavelWebResponse(req);
			if (res.Success && res.DataByte.Length > 0) {
				asset.SetData(res.DataByte);
				
				onComplete?.Invoke(asset);
			}
			else {
				Debug.LogError(res.Error.FullMessage);
				onFailure?.Invoke(res.Error.FullMessage);
			}
		}
		else {
			asset.Overwrite(newAsset);
		}
	}

	/// <summary>
	/// Retrieve asset from a specific slot, in the current space.
	/// </summary>
	/// <param name="channelId">identifier of the call/channel in which the slot resides.</param>
	/// <param name="slotId">Id of the slot to retrieve the asset for.</param>
	/// <param name="onComplete">Callback for when data has been retrieved, includes asset.</param>
	/// <param name="onFailure">Callback for failure, includes error string.</param>
	public IEnumerator RetrieveChannelSlotAsset(string channelId, string slotId, Action<Asset> onComplete, Action<string> onFailure) {
		throw new Exception("Missing call exception, retrieve slot data call not yet implemented!");
		
		/*RavelWebRequest req = SpaceRequest.GetSpace(DataServices.Spaces.Current.id);
		yield return req.Send();

		RavelWebResponse res = new RavelWebResponse(req);
		if (res.Success && res.TryGetData(out Space curSpace)) {
			DataServices.Spaces.Current.Overwrite(curSpace);

			if (DataServices.Spaces.Current.TryGetSlotAsset(slotId, out Asset asset)) {
				onComplete?.Invoke(asset);
			}
			else {
				Debug.LogError($"Could not find slot with slotID: {slotId}");
				onFailure?.Invoke($"Could not find slot with slotID: {slotId}");
			}
		}
		else {
			onFailure?.Invoke($"Error retrieving current space: {DataServices.Spaces.Current.id}");*/
	}
	
	/// <summary>
	/// Publish asset to a specific slot, in given channel.
	/// </summary>
	/// <param name="channelId">identifier of the call/channel to which the asset is pushed.</param>
	/// <param name="slotId">Id of the slot to publish the asset to.</param>
	/// <param name="onComplete">Callback for when data has been published.</param>
	/// <param name="onFailure">Callback for failure, includes error string.</param>
	public IEnumerator PublishSpaceSlotAsset(string channelId, string slotId, Asset asset, Action onComplete, Action<string> onFailure) {
		throw new Exception("Missing call exception, publish slot data call not yet implemented!");
		
		/*RavelWebRequest req = SpaceRequest.PublishSpaceSlot(DataServices.Spaces.Current.id, slotName, asset.id);
		yield return req.Send();

		RavelWebResponse res = new RavelWebResponse(req);
		if (res.Success) {
			onComplete?.Invoke();
		}
		else {
			Debug.LogError(res.Error.FullMessage);
			onFailure?.Invoke(res.Error.FullMessage);
		}*/
	}
}
