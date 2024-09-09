using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cohort.Ravel.Patterns
{
    public class ObjectPool<TData, TType> where TType : ObjectPool<TData, TType>.IPoolable
    {
        public int ActiveCount {
            get { return _active.Count; }
        }

        public List<TType> Active {
            get { return _active; }
        }

        private List<TType> _active;
        private List<TType> _inactive;

        private TType _template;

        /// <summary>
        /// Create new object pool, using given template as prefab
        /// </summary>
        /// <param name="template">template for making new objects</param>
        /// <param name="addTemplate">keep template inactive, but use it to copy new instances from.</param>
        public ObjectPool(TType template, bool addTemplate = true)
        {
            _active = new List<TType>();
            _inactive = new List<TType>();

            _template = template;
            _template.IsActive = false;
            
            if (addTemplate) {
                AddItem(_template);    
            }
        }

        /// <summary>
        /// Adds item to the pool, active if item active, inactive if not
        /// </summary>
        /// <param name="item">item to add, doesn't update active or values, change those before passing the item.</param>
        public void AddItem(TType item)
        {
            if (item.IsActive) {
                _active.Add(item);
            }
            else {
                _inactive.Add(item);
            }
        }
        
        /// <summary>
        /// Check pool, if empty, add new, otherwise use existing object.
        /// </summary>
        /// <param name="data">data for object.</param>
        /// <returns>spawned object.</returns>
        public TType AddItem(TData data)
        {
            TType item;
            if (_inactive.Count > 0) {
                item = _inactive[0];
                _inactive.RemoveAt(0);
            }
            else {
                item = _template.Copy();
            }

            item.UpdatePoolable(_active.Count, data);
            item.IsActive = true;
            _active.Add(item);
            return item;
        }

        /// <summary>
        /// removes item with id out of the active list 
        /// </summary>
        public void RemoveItem(int id)
        {
            if (id >= 0 && id < _active.Count) {
                TType type = _active[id];
                _active.RemoveAt(id);
                type.IsActive = false;

                _inactive.Add(type);
            }
            else {
                throw new IndexOutOfRangeException($"id {id} is out of range for list of length ({_active.Count})");
            }
        }

        public void RemoveItem(TType item)
        {
            _active.Remove(item);
            item.IsActive = false;
            _inactive.Add(item);
        }

        public void DestroyItem(TType item)
        {
            if (_active.Contains(item)) {
                _active.Remove(item);
            } else if (_inactive.Contains(item)) {
                _inactive.Remove(item);
            }
            else {
                Debug.LogWarning($"destroying {item} from pool, item not found!");
            }
        }

        public void SetAll(IEnumerable<TData> data)
        {
            int i = 0;
            //update all entries, keeping count with i
            foreach (TData entry in data) {
                if (i < _active.Count) {
                    //update active item
                    TType t =_active[i];
                    t.UpdatePoolable(i, entry);
                    t.IsActive = true;
                    _active[i] = t;
                }
                else {
                    //create new/ inactive retieve
                    AddItem(entry);
                }
                i++;
            }
            
            //if active still has entries that are not updated, disable them.
            for (; i < _active.Count; i++) {
                //disable active
                RemoveItem(i);
                //reduce index so next element in list is selected as previous one has been removed.
                i--;
            }
        }

        public void SetAll(TData[] data)
        {
            for (int i = 0; i < Mathf.Max(_active.Count, data.Length); i++) {
                if (i < _active.Count && i < data.Length) {
                    //update active item
                    TType t =_active[i];
                    t.UpdatePoolable(i, data[i]);
                    t.IsActive = true;
                    _active[i] = t;
                }
                else if (i < data.Length){
                    //create new/ inactive retieve
                    AddItem(data[i]);
                }
                else {
                    //disable active
                    RemoveItem(i);
                    //reduce index so next element in list is selected as previous one has been removed.
                    i--;
                }
            }
        }

        /// <summary>
        /// Clears all internal references, but does not destroy the items.
        /// </summary>
        public void ClearAllReferences()
        {
            _active.Clear();
            _inactive.Clear();
        }
        
        
        public interface IPoolable
        {
            public bool IsActive { get; set; }
            
            public void UpdatePoolable(int index, TData data);

            public TType Copy();
        }
    }
}