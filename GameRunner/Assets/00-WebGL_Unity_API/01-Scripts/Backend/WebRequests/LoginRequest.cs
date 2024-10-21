using System;
using System.Text;
using UnityEngine;

namespace Cohort.Networking.Authorization
{
    /// <summary>
    /// Overlaying class to retrieve specific web requests from the login subset of the backend.
    /// </summary>
    public class LoginRequest : RavelWebRequest
    {
        /// <summary>
        /// Key to use when retrieving old access tokens from playerprefs. 
        /// </summary>
        public const string SYSTEMS_TOKEN_KEY = "SYSTEMS_TOKEN";
        
        /// <summary>
        /// Base constructor for login webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/auth/</param>
        /// <param name="json">serialized json data to add to the webrequest body.</param>
        /// <param name="api">Left empty, no api part in these urls.</param>
        /// <param name="version">version modifier to add to the called url, v1/ by default.</param>
        protected LoginRequest(string postfix, string json, string api = "api/") : base(Method.PostJSON, api)
        {
            _data = json;
            _url += "auth/" + postfix;
        }

        /// <summary>
        /// Request token using login data (PC).
        /// </summary>
        public static LoginRequest LoginTokenRequest(string token)
        {
            string json = JsonUtility.ToJson(new LoginProxy(token));
            return new LoginRequest("login", json);
        }
        
        [Serializable]
        public class SimpleToken {
            public string token;
        }
        
        /// <summary>
        /// Token wrapper and handler class.
        /// </summary>
        [Serializable]
        public class JWTToken {
            //date from which the JWT tokens count themselves
            private readonly DateTime NUL_DATE = new (1970, 1, 1);
            //reduce time by 10 seconds to ensure token expires a bit before actual expiary
            private const int SAFETY_BUFF = 10;
            
            public string token;
            [SerializeField] private int _expSec; 
            
            private DateTime _expire;
            private bool _expireSet;
            
            public DateTime GetExpiary() {
                if (!_expireSet) {
                    _expire = NUL_DATE + new TimeSpan(0, 0, _expSec);
                    _expireSet = true;
                }

                return _expire;
            }
            
            public void Decode() {
                string base64 = token.Split('.')[1];
                base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
                
                TokenInternals internals =
                    JsonUtility.FromJson<TokenInternals>(
                        Encoding.UTF8.GetString(Convert.FromBase64String(base64)));
                
                int diff = internals.exp - internals.iat;
                //this is secretly not the actual expiary, but we use it as a buffer.
                _expire = DateTime.Now + new TimeSpan(0,0,0,diff - SAFETY_BUFF);
                _expSec = (int)(_expire - NUL_DATE).TotalSeconds;
            }

            /// <summary>
            /// Decoded token data from server.
            /// https://en.wikipedia.org/wiki/JSON_Web_Token#Structure
            /// </summary>
            [Serializable]
            private class TokenInternals {
                //expiary
                public int exp;
                //issued at
                public int iat;
            }
        }

        /// <summary>
        /// Proxy to send to the server, containing the user data (is parsed through json and put into the body).
        /// </summary>
        [Serializable]
        private class LoginProxy
        {
            [SerializeField]
            private string token;

            public LoginProxy(string token)
            {
                this.token = token;
            }
        }
    }
}