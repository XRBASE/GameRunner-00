using UnityEngine;

namespace Cohort.Networking {
    /// <summary>
    /// Class for creating a list with webcall data proxies.
    /// </summary>
    /// <typeparam name="T">proxy type.</typeparam>
    public class ProxyCollection<T>
    {
        public int Length {
            get { return proxies.Length; }
        }

        public T this[int i] {
            get { return proxies[i]; }
            private set { proxies[i] = value; }
        }

        /// <summary>
        /// returns ref, NO COPY!
        /// </summary>
        public T[] Array {
            get { return proxies; }
        }

        public T[] proxies;
        private string _collectionFieldName;
        
        public ProxyCollection(string collectionFieldName = "proxies")
        {
            
        }

        public static ProxyCollection<T> Get(string data, string collectionFieldName = "proxies") {
            if (string.IsNullOrEmpty(collectionFieldName))
                collectionFieldName = "proxies";
            
            bool f = data[0] == '[';
            bool l = data[data.Length - 1] == ']';
            if(!f) {
                data = "[" + data;
            }

            if(!l) {
                data = data + "]";
            }
            
            if (collectionFieldName == "proxies")
            {
                //If the field name is proxies, the json data will be encapsulated, this is the case for array jsons
                //all json data will be put into the proxies array 
                //create overlying class with containing all proxies
                data = "{\"" + collectionFieldName + "\":" + data + "}";
            }
            else {
                //if a field name is given, the assumption is made that an array is contained within the json value, which is 
                //the data we want to put into the proxies array, so we replace the name of that field with the name proxies, so the
                //data will be saved into the proxies array
                data.Replace(collectionFieldName, "proxies");
            }
            

            try {
                return JsonUtility.FromJson<ProxyCollection<T>>(data);
            } catch {
                Debug.LogError("Faultive JSON recieved!");
                Debug.LogError($"value: {data}");
                return null;
            }
        }
        
        public static bool TryGet(string data, out ProxyCollection<T> collection, string collectionFieldName = "proxies") {
            if (collectionFieldName == "proxies")
            {
                bool f = data[0] == '[';
                bool l = data[data.Length - 1] == ']';
                if(!f) {
                    data = "[" + data;
                }

                if(!l) {
                    data = data + "]";
                }
                
                //If the field name is proxies, the json data will be encapsulated, this is the case for array jsons
                //all json data will be put into the proxies array 
                //create overlying class with containing all proxies
                data = "{\"proxies\":" + data + "}";
            }
            else {
                //if a field name is given, the assumption is made that an array is contained within the json value, which is 
                //the data we want to put into the proxies array, so we replace the name of that field with the name proxies, so the
                //data will be saved into the proxies array
                data = data.Replace(collectionFieldName, "proxies");
            }

            try {
                collection = JsonUtility.FromJson<ProxyCollection<T>>(data);
                return true;
            } catch {
                collection = null;
                Debug.LogError("Faultive JSON recieved!");
                Debug.LogError($"value: {data}");
                return false;
            }
        }

        public static string GetCsv(ProxyCollection<T> collection)
        {
            string csv = "";
            for(int i = 0; i < collection.Length; i++) {
                if(i < collection.Length - 1) {
                    csv += collection[i].ToString() + ",";
                } else {
                    csv += collection[i].ToString();
                }
            }
            return csv;
        }

        public void Clear() {
            proxies = System.Array.Empty<T>();
        }
    }
}
