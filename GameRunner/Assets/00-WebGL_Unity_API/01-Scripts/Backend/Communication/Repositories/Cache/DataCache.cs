using System;
using System.Collections.Generic;
using Cohort.Ravel.BackendData.Data;

public class DataCache<T> where T : DataContainer
{
    /// <summary>
    /// This get method may only be used if it's 100% sure the item has already been cached and updated appropriately, otherwise
    /// use the TryGet method
    /// </summary>
    /// <param name="key">key under which item is saved.</param>
    public T this[string key] {
        get { return _entries[key].item; }
    }

    // Duration of which an item is valid in this repo's cache.
    private TimeSpan _validityPeriod;
    
    //The actual cache dictionary
    protected Dictionary<string, DataEntry> _entries;

    public DataCache(TimeSpan validityPeriod)
    {
        _validityPeriod = validityPeriod;
        _entries = new Dictionary<string, DataEntry>();
    }

    /// <summary>
    /// Get list of keys of items in cache
    /// </summary>
    public IEnumerable<string> GetKeys()
    {
        return _entries.Keys;
    }

    /// <summary>
    /// Check if item with key exists in dictionary.
    /// </summary>
    /// <param name="key">key of item.</param>
    public bool ContainsKey(string key)
    {
        return _entries.ContainsKey(key);
    }
    
    /// <summary>
    /// Save an item in the cache.
    /// </summary>
    /// <param name="key">key for the item.</param>
    /// <param name="item">item to cache.</param>
    public void CacheItem(string key, T item)
    {
        if (_entries.ContainsKey(key)) {
            //overwrite existing entry with any fields that are contained within item. 
            _entries[key].Overwrite(item);
        }
        else {
            _entries.Add(key, new DataEntry(item));
        }
    }

    /// <summary>
    /// Updates item's value, without updating the validity date.
    /// </summary>
    /// <param name="key">key for the item.</param>
    /// <param name="item">item to cache.</param>
    public void UpdateItemWithoutDate(string key, T item)
    {
        if (_entries.ContainsKey(key)) {
            _entries[key].Overwrite(item, false);
        }
    }

    /// <summary>
    /// Remove item from cache.
    /// </summary>
    /// <param name="key">key under which the item has been saved.</param>
    public bool UncacheItem(string key)
    {
        if (_entries.ContainsKey(key)) {
            _entries.Remove(key);
            return true;
        }
        
        return false;
    }

    public void ClearCache()
    {
        _entries.Clear();
    }

    /// <summary>
    /// Returns all cached data entries, independent of date.
    /// </summary>
    public T[] GetAllData()
    {
        T[] data = new T[_entries.Count];
        int i = 0;
        foreach (var pair in _entries) {
            data[i] = pair.Value.item;
            i++;
        }

        return data;
    }
    
    /// <summary>
    /// Returns all cached data entries, independent of date.
    /// </summary>
    public List<T> GetDataList()
    {
        List<T> data = new List<T>(_entries.Count);
        foreach (var pair in _entries) {
            data.Add(pair.Value.item);
        }

        return data;
    }

    /// <summary>
    /// Tries to find an item in the cache, fails if no item found, or the validity period has been expired.
    /// </summary>
    /// <param name="item">item to find in cache</param>
    public bool TryGetCachedItem(string key, out T item)
    {
        if (_entries.ContainsKey(key) && !ItemExpired(key)) {
            item = _entries[key].item;
            return true;
        }

        item = default(T);
        return false;
    }

    /// <summary>
    /// Check if collection is contained in cache.
    /// </summary>
    /// <param name="keys">keys of collection.</param>
    public bool HasCollection(string[] keys)
    {
        for (int i = 0; i < keys.Length; i++) {
            if (!_entries.ContainsKey(keys[i]))
                return false;
        }

        return true;
    }
    
    /// <summary>
    /// Tries to get all items in a collection from the cache,
    /// if one or multiple items are not found, the ones that are
    /// found are returned.
    /// </summary>
    /// <param name="items">list of items to find, no found items will be removed in the method.</param>
    public bool TryGetCachedCollection(string[] keys, out List<T> items)
    {
        items = new List<T>();
        bool foundAll = true;
        for (int i = 0; i < keys.Length; i++) {
            T found;
            if (TryGetCachedItem(keys[i], out found)) {
                items.Add(found);
            }
            else if (foundAll) {
                foundAll = false;
            }
        }

        return foundAll;
    }

    /// <summary>
    /// Check whether item imported on date would be valid, with the validity period of this cache.
    /// </summary>
    /// <param name="date">date to check.</param>
    /// <returns></returns>
    public bool DateTimeValid(DateTime date)
    {
        return date + _validityPeriod >= DateTime.Now;
    }
    
    /// <summary>
    /// Check if item in cache has expired its validity period.
    /// This method does not check if the key is contained, do that before calling it.
    /// </summary>
    /// <param name="key">the key under which the item has been saved.</param>
    private bool ItemExpired(string key)
    {
        return _entries[key].cacheDate + _validityPeriod < DateTime.Now;
    }
    
    /// <summary>
    /// Simple class in which to save an entry and it's cache date
    /// </summary>
    protected class DataEntry
    {
        public T item;
        public DateTime cacheDate;

        public DataEntry(T item)
        {
            this.item = item;
            cacheDate = DateTime.Now;
        }

        public void Overwrite(T item, bool writeDate = true)
        {
            this.item.Overwrite(item);
            if (writeDate)
                cacheDate = DateTime.Now;
        }
    }
}
