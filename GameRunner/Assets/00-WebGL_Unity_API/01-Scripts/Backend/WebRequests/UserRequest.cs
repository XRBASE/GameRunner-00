using UnityEngine;
using System;

namespace Cohort.Networking.Users
{
    /// <summary>
    /// Overlaying class to retrieve specific web requests from the user subset of the backend.
    /// </summary>
    public class UserRequest : TokenWebRequest
    {
        /// <summary>
        /// Base constructor for user webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="method">Method of webcall (Post, Get, Put, etc)</param>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/users/</param>
        /// <param name="version">Version modifier to add to the called url, v1/ by default.</param>
        private UserRequest(Method method, string postfix) : base(method, "api/")
        {
            _url += postfix;
        }
        
        /// <summary>
        /// Request local user's data.
        /// </summary>
        public static UserRequest GetMe() {
            return new UserRequest(Method.Get, "auth/me");
        }
        
        /// <summary>
        /// Update users avatar url.
        /// </summary>
        public static UserRequest UpdateAvatarUrl(string data) {
            AvatarProxy proxy = new AvatarProxy();
            proxy.avatarData = data;

            string json = JsonUtility.ToJson(proxy);
            
            UserRequest req = new UserRequest(Method.PatchJSON, $"auth/me");
            req._data = json;

            return req;
        }

        [Serializable]
        private class AvatarProxy {
            public string avatarData;
        }
    }
}