using Cohort.Config;

namespace Cohort.Networking.Files
{
    /// <summary>
    /// Overlaying class to retrieve specific web requests from the files subset of the backend.
    /// </summary>
    public class FileRequest : TokenWebRequest
    {
        /// <summary>
        /// Base constructor for file webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="method">method of webcall (Post, Get, Put, etc)</param>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/userfiles/</param>
        /// <param name="version">version modifier to add to the called url, v1/ by default.</param>
        private FileRequest(Method method, string postfix, string version = "v1/") : base(method, "api/")
        {
            _url += $"userfiles/{postfix}";
        }

        /// <summary>
        /// Get array of File's that match the users uploaded files.
        /// </summary>
        public static FileRequest GetFilesForUser()
        {
            return new FileRequest(Method.Get, "");
        }
        
        /// <summary>
        /// Get download request to retrieve download signed url for a specific file. This in the form of a Ravel webrequest.
        /// </summary>
        /// <param name="fileId">id (see File class) of the file that needs to be downloaded.</param>
        public static FileRequest GetDownloadRequest(string fileId)
        {
            return new FileRequest(Method.Get, $"request/{fileId}", "v2/");
        }

        /// <summary>
        /// Get download url (with which signed url can be retrieved), in string format.
        /// </summary>
        /// <param name="fileId">ID of file, of which the url is being retrieved.</param>
        public static string GetDownloadUrl(string fileId) {
            return AppConfig.Config.BaseUrl + $"api/v2/userfiles/request/{fileId}";
        }

        /// <summary>
        /// Json proxy for GetDownloadURL response.
        /// </summary>
        [System.Serializable]
        public class FileURLResponse
        {
            public string preSignedUrl;
        }
    }
}
