using Cohort.BackendData;

using System.Collections.Generic;
using System;

/// <summary>
/// Repository containing backend calls (inheritors) and data caching capabilities (this class).
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DataRepository<T> where T: DataContainer
{
    //date that is only refreshed when all data is retrieved, so that cache can also be used in cases which require all data.
    protected DateTime _lastAllRetrievedDate;
    protected bool _useCashe = true;
    private DataCache<T> _cache;
    
    public DataRepository(bool useCashe, TimeSpan validityPeriod)
    {
        _lastAllRetrievedDate = DateTime.MinValue;
        
        //TODO: Backend 2.0 retrieve timespan from config file
        this._useCashe = useCashe;
        if (useCashe) {
            DataServices.Login.onUserLoggedOut += ClearCache;
            _cache = new DataCache<T>(validityPeriod);    
        }
    }

    /// <summary>
    /// Remove all entries from the cache.
    /// </summary>
    protected void ClearCache()
    {
        if (!_useCashe) {
            return;
        }
        
        _lastAllRetrievedDate = DateTime.MinValue;
        _cache.ClearCache();
    }
    
    public bool ContainsKey(string key)
    {
        return _cache.ContainsKey(key);
    }

    /// <summary>
    /// Try to get specific item out of cache.
    /// </summary>
    /// <param name="keyholder">item containing the key for the searched item</param>
    /// <param name="result">item found in the cache.</param>
    /// <returns>true/false a result was found in the cache and that result was still valid.</returns>
    public bool TryGetFromCache(T keyholder, out T result)
    {
        if (!_useCashe) {
            result = null;
            return false;
        }
        
        if (keyholder == null) {
            result = null;
            return false;
        }
        
        return TryGetFromCache(keyholder.Key, out result);
    }
    
    /// <summary>
    /// Try to get specific item out of cache.
    /// </summary>
    /// <param name="key">the key for the searched item.</param>
    /// <param name="result">item found in the cache.</param>
    /// <returns>true/false a result was found in the cache and that result was still valid.</returns>
    public bool TryGetFromCache(string key, out T result)
    {
        if (!_useCashe) {
            result = null;
            return false;
        }
        
        return _cache.TryGetCachedItem(key, out result); 
    }

    /// <summary>
    /// Try to get all data from the cache, fails if last all retrieved date is too old.
    /// </summary>
    /// <param name="data">Retrieved data from cache.</param>
    /// <param name="useDate">Check if last retrieved date has expired.</param>
    public bool TryGetAllFromCache(out T[] data, bool useDate = true)
    {
        if (!_useCashe || (useDate && !_cache.DateTimeValid(_lastAllRetrievedDate))) {
            //invalid early-escape
            data = null;
            return false;
        }
        
        data = _cache.GetAllData();
        return true;
    }
    
    /// <summary>
    /// Try to get all data from the cache, fails if last all retrieved date is too old.
    /// </summary>
    /// <param name="data">retrieved data from cache.</param>
    public bool TryGetAllFromCache(out List<T> data)
    {
        if (!_useCashe || !_cache.DateTimeValid(_lastAllRetrievedDate)) {
            //invalid early-escape
            data = null;
            return false;
        }
        
        data = _cache.GetDataList();
        return true;
    }

    /// <summary>
    /// Caches data without changing the date in the cache.
    /// </summary>
    /// <param name="item">data to change.</param>
    public void CacheDataDateless(T item)
    {
        if (!_useCashe) {
            return;
        }
        
        if (_useCashe) { 
            _cache.UpdateItemWithoutDate(item.Key, item);
        }
    }
    
    /// <summary>
    /// Add single entry to cache.
    /// </summary>
    /// /// <param name="writeDate">update existing entries date stamp</param>
    public void CacheData(T data)
    {
        if (!_useCashe) {
            return;
        }
        
        _cache.CacheItem(data.Key, data);
    }

    /// <summary>
    /// Add multiple entries to cache.
    /// </summary>
    /// <param name="writeDates">update existing entries date stamp</param>
    public void CacheDataRange(T[] data)
    {
        if (!_useCashe) {
            return;
        }

        for (int i = 0; i < data.Length; i++) {
            CacheData(data[i]);
        }
    }
    
    /// <summary>
    /// Add multiple entries to cache.
    /// </summary>
    /// <param name="writeDates">update existing entries date stamp</param>
    public void CacheDataRange(List<T> data)
    {
        if (!_useCashe) {
            return;
        }

        for (int i = 0; i < data.Count; i++) {
            CacheData(data[i]);
        }
    }

    /// <summary>
    /// Remove item from the cache.
    /// </summary>
    /// <param name="key">key under which the item is saved.</param>
    public bool RemoveFromCache(string key)
    {
        if (!_useCashe) {
            return false;
        }
        
        return _cache.UncacheItem(key);
    }
}
