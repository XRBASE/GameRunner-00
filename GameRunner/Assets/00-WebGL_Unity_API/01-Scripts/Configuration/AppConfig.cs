using UnityEngine;

namespace Cohort.Config
{
    [CreateAssetMenu(menuName = "Cohort/Config", fileName = "CohortConfig", order = 0)]
    public class AppConfig : ScriptableObject {
        public static AppConfig Config {
            get {
                if (_instance == null) {
                    _instance = Resources.Load<AppConfig>("Config/CohortConfig");
                }

                return _instance;
            }
        }

        private static AppConfig _instance;

        public string BaseUrl {
            get { return _baseUrl; }
        }

        public string AppId {
            get { return _appId; }
        }

        [SerializeField] private string _baseUrl;
        [SerializeField] private string _appId;

        public void OverrideBaseUrl(string url) {
            _baseUrl = url;
        }
        
        public void OverrideAppID(string id) {
            _appId = id;
        }
        
        /// <summary>
        /// Returns the value before the fist dot of the version number (major number)
        /// </summary>
        public static string GetMajorVersionNumber()
        {
            string version = Application.version;
            if (Application.version.Contains(".")) {
                int length = version.IndexOf('.');
                version = version.Substring(0, length);
            }

            return version;
        }
    }
}