using Cohort.Tools.Timers;
using Cohort.Assets;

using System.Collections.Generic;
using UnityEngine;
using System;

public class AssetService : MonoBehaviour
{
    //interval between dynacontent updates 
    private const float UPDATE_DURATION = 20f;
    
    public Action onFileSlotUpdate;
    
    private AssetRepository _repo;
    private Timer _updateTimer;
    private bool _autoUpdate = true;

    private void Awake() {
        _repo = new AssetRepository();

        _updateTimer = new Timer(UPDATE_DURATION, false, UpdateSlots);
    }
    
    /// <summary>
    /// Gets all (personal)assets of the currently logged in user.
    /// </summary>
    /// <param name="onComplete">callback for when assets have been retrieved.</param>
    /// <param name="onFailure">callback for when something has gone wrong retrieving assets (passes error string).</param>
    public void GetAllUserAssets(Action<Asset[]> onComplete = null, Action<string> onFailure = null, FileType filter = ~(FileType.none)) {
        StartCoroutine(_repo.RetrieveUserAssets((assets) => FilterAssets(assets, onComplete, filter), onFailure));
    }

    private void FilterAssets(Asset[] assets, Action<Asset[]> onComplete, FileType filter = ~(FileType.none)) {
        List<Asset> filtered = new List<Asset>();
        for (int i = 0; i < assets.Length; i++) {
            if ((assets[i].Type & filter) > 0) {
                filtered.Add(assets[i]);
            }
        }
        
        onComplete?.Invoke(filtered.ToArray());
    }

    /// <summary>
    /// Download the data that is associated with the given asset file.
    /// </summary>
    /// <param name="asset">asset of which the data is being downloaded.</param>
    /// <param name="onComplete">callback for returning the asset, including data.</param>
    /// <param name="onFailure">callback when action fails, including error string.</param>
    public void DownloadAssetData(Asset asset, Action<Asset> onComplete = null, Action<string> onFailure = null) {
        StartCoroutine(_repo.RetrieveAssetData(asset, onComplete, onFailure));
    }

    /// <summary>
    /// Find asset slot content file of the current space
    /// </summary>
    /// <param name="channelId">identifier of the call/channel in which the slot resides.</param>
    /// <param name="slotId">string identifier of the slot that the system searches for.</param>
    /// <param name="onComplete">callback to return asset when it has been found.</param>
    /// <param name="onFailure">callback to return the error when something goes wrong.</param>
    public void GetChannelSlotAsset(string channelId, string slotId, Action<Asset> onComplete, Action<string> onFailure = null) {
        StartCoroutine(_repo.RetrieveChannelSlotAsset(channelId, slotId, onComplete, onFailure));
    }
    
    

    /// <summary>
    /// Set asset to slot in current space.
    /// </summary>
    /// <param name="channelId">identifier of the call/channel in which the slot resides.</param>
    /// <param name="slotName">Name (of loader) under which the changes are saved.</param>
    /// <param name="asset">Asset that is being assigned to the slot (empty asset for no asset).</param>
    /// <param name="onComplete">Callback for when changes have been accepted.</param>
    /// <param name="onFailure">Callback when changes are not accepted and there is an error (includes error message string).</param>
    public void SetSlotAsset(string channelId, string slotName, Asset asset, Action onComplete = null, Action<string> onFailure = null) {
        StartCoroutine(_repo.PublishSpaceSlotAsset(channelId, slotName, asset, onComplete, onFailure));
    }

    public void StartSlotUpdate() {
        if (!_autoUpdate && !_updateTimer.Active) {
            _updateTimer.Reset();
            _updateTimer.Start();
        }

        _autoUpdate = true;
    }
    
    public void StopSlotUpdate() {
        if (_autoUpdate && _updateTimer.Active) {
            _updateTimer.Stop();
        }

        _autoUpdate = false;
    }
    
    private void UpdateSlots() {
        _updateTimer.Reset();
        if (!_updateTimer.Active) {
            _updateTimer.Start();
        }
        
        //triggers all file loaders to check their name against current space for slot updates.
        onFileSlotUpdate?.Invoke();
    }
}
