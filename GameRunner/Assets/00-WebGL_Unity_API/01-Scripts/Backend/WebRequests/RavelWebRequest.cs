using System;
using System.Collections.Generic;
using System.Text;
using Cohort.Config;
using Cohort.Ravel.Networking.Authorization;
using UnityEngine;
using UnityEngine.Networking;

namespace Cohort.Ravel.Networking {
    
    /// <summary>
    /// Base class for overriding and creating web requests.
    /// </summary>
    public class RavelWebRequest {
        public string Url {
            get { return _url; }
        }

        public UnityWebRequest Request {
            get { return _request; }
        }
        
        public Method CallMethod {
            get { return _method; }
        }

        public InternalError Error {
            get { return _error; }
        }

        protected string _url;
        protected UnityWebRequest _request;
        protected Dictionary<string, string> _parameters;
        protected Dictionary<string, string> _header; 
        protected WWWForm _form;
        //used for passing data in PostJSON calls
        protected string _data = "";
        protected Method _method;
        protected InternalError _error = InternalError.None;
        
        private bool _disposed;

        /// <summary>
        /// Fill it in yourself request, doesn't auto set anything, not defined in constructor parameters.
        /// </summary>
        /// <param name="url">Url to which to make the request (full)</param>
        /// <param name="method">Method of requesting (Get, Post, Put, Etc).</param>
        /// <param name="parameters">Parameters to add to the end of the url.</param>
        /// <param name="header">Request headers.</param>
        public RavelWebRequest(string url, Method method, Dictionary<string, string> parameters = null, Dictionary<string, string> header = null) {
            if(header != null) {
                _header = header;
            } else {
                _header = new Dictionary<string, string>();
            }

            if(parameters != null) {
                _parameters = parameters;
            } else {
                _parameters = new Dictionary<string, string>();
            }

            _method = method;
            _url = url;
            _disposed = false;
        }
        
        /// <summary>
        /// Form based put request. binary data can be added to the form to push it the url server. This calls the url
        /// without use of the base url.
        /// </summary>
        /// <param name="url">full url to which the webcall is made.</param>
        /// <param name="form">form for webdata to upload.</param>
        /// <param name="parameters">parameters (added to url)</param>
        /// <param name="header">headers for webrequest.</param>
        public RavelWebRequest(string url, WWWForm form, Dictionary<string, string> parameters = null, Dictionary<string, string> header = null) {
            if(header != null) {
                _header = header;
            } else {
                _header = new Dictionary<string, string>();
            }

            if(parameters != null) {
                _parameters = parameters;
            } else {
                _parameters = new Dictionary<string, string>();
            }
            
            _url = url;
            _method = Method.Post;
            _form = form;
            _disposed = false;
        }

        /// <summary>
        /// Internal form based post call.
        /// </summary>
        /// <param name="api">api text that is added to the url (baseUrl/api/postfix).</param>
        /// <param name="form">form with request data.</param>
        protected RavelWebRequest(string api, WWWForm form) {
            _parameters = new Dictionary<string, string>();
            _header = new Dictionary<string, string>();
            _method = Method.Post;
            
            _url = AppConfig.Config.BaseUrl + api;
            _form = form;
            _disposed = false;
        }

        /// <summary>
        /// Base for Ravel web requests, fills in the base url, based on development setting. 
        /// </summary>
        /// <param name="method">Method of requesting (Get, Post, Put, Etc).</param>
        /// <param name="api">Api addition part, added between base url and postfix.</param>
        protected RavelWebRequest(Method method, string api) {
            _parameters = new Dictionary<string, string>();
            _header = new Dictionary<string, string>();
            _method = method;
            
            _url = AppConfig.Config.BaseUrl + api;
            _disposed = false;
        }
        
        ~RavelWebRequest()
        {
            if (!_disposed) {
                DisposeData();    
            }
        }
        
        public void DisposeData()
        {
            _disposed = true;
            
            if (_request != null) {
                _request.uploadHandler?.Dispose(); 
                _request.downloadHandler?.Dispose();
            }

            _form = null;
        }

        /// <summary>
        /// ets up the actual webrequest, adds the parameters and headers and returns this. Used just before sending the webrequest.
        /// </summary>
        /// <param name="jsonData">data string for JSON post request, otherwise leave empty.</param>
        protected virtual void GetRequest() {
            switch (_method) {
                case Method.Get:
                    _url += ParameterExtension;
                    _request = UnityWebRequest.Get(_url);
                    break;
                case Method.GetSprite:
                    _request = new UnityWebRequest(_url, "GET");
                    _request.downloadHandler = new DownloadHandlerTexture(true);
                    break;
                case Method.Post:
                    if (_form != null) {
                        _url += ParameterExtension;
                        _request = UnityWebRequest.Post(_url, _form);
                    }
                    else {
                        _request = UnityWebRequest.Post(_url, _parameters);
                    }
                    break;
                case Method.PostJSON:
                    _url += ParameterExtension;
                    //specific case for posting json data
                    _request = new UnityWebRequest(_url, "POST");
        
                    //use upload and download manager manually to enforce json instead of form
                    _request.uploadHandler = (UploadHandler)new UploadHandlerRaw(new UTF8Encoding().GetBytes(_data));
                    _request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    _request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case Method.PatchJSON:
                    _url += ParameterExtension;
                    //specific case for posting json data
                    _request = new UnityWebRequest(_url, "PATCH");
        
                    //use upload and download manager manually to enforce json instead of form
                    _request.uploadHandler = (UploadHandler)new UploadHandlerRaw(new UTF8Encoding().GetBytes(_data));
                    _request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    _request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case Method.Put:
                    _request = new UnityWebRequest(_url, "PUT");
                    //use upload and download manager manually to enforce json instead of form
                    _request.uploadHandler = (UploadHandler)new UploadHandlerRaw(new UTF8Encoding().GetBytes(_data));
                    _request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    _request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case Method.Delete:
                    _request = UnityWebRequest.Delete(_url);
                    break;
                case Method.DeleteJSON:
                    //specific case for posting json data
                    _request = new UnityWebRequest(_url, "DELETE");
        
                    //use upload and download manager manually to enforce json instead of form
                    _request.uploadHandler = (UploadHandler)new UploadHandlerRaw(new UTF8Encoding().GetBytes(_data));
                    _request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                    _request.SetRequestHeader("Content-Type", "application/json");
                    break;
                case Method.GetAssetBundle:
                    _request = UnityWebRequestAssetBundle.GetAssetBundle(_url);
                    break;
                default:
                    throw new NotImplementedException($"({_method}) request not implemented yet");
            }

            if(_header != null) {
                foreach (KeyValuePair<string, string> pair in _header)
                {
                    _request.SetRequestHeader(pair.Key, pair.Value);
                }
            }
        }

        /// <summary>
        /// Set's up (GetWebRequest, see code above) and sends the webrequest.
        /// </summary>
        /// <returns>Async operation for sending the request.</returns>
        public virtual AsyncOperation Send() {
            GetRequest();
            return _request.SendWebRequest();
        }

        /// <summary>
        /// Add singular parameter to parameter list.
        /// </summary>
        /// <param name="key">parameter name</param>
        /// <param name="value">parameter value</param>
        public void AddParameter(string key, string value) {
            
            _parameters.Add(key, value);
        }

        /// <summary>
        /// Add multiple parameters to the parameter list.
        /// </summary>
        /// <param name="parameters">Dictionary filled witn name, value parameters.</param>
        public void AddParameters(Dictionary<string, string> parameters) {
            if (parameters == null) {
                return;
            }
            foreach(var pair in parameters) {
                this._parameters.Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Getter for string version of parameters (placed behind url to pass parameters).
        /// </summary>
        protected string ParameterExtension {
            get {
                string ext = "";
                if(_parameters == null || _parameters.Count <= 0) {
                    return ext;
                }

                int c = 0;
                foreach(KeyValuePair<string, string> pair in _parameters) {
                    if(c == 0) {
                        ext += "?";
                    }
                    else
                    {
                        ext += "&";
                    }
                    ext += $"{pair.Key}={pair.Value}";
                    c++;
                }

                return ext;
            }
        }

        /// <summary>
        /// Add singular header to the list of headers.
        /// </summary>
        /// <param name="key">Name of the header to add.</param>
        /// <param name="value">Value of header to add</param>
        public void AddHeader(string key, string value) { _header.Add(key, value); }

        /// <summary>
        /// Method of requesting data from backend.
        /// </summary>
        public enum Method
        {
            Get,
            GetSprite,
            Post,
            PostJSON,
            PatchJSON,
            PostBytes,
            Put,
            Delete,
            DeleteJSON,
            GetAssetBundle
        }

        public enum InternalError
        {
            None = 0,
            MissingToken = 1,
        }
    }

    /// <summary>
    /// Any webrequest using the Ravel backend token.
    /// </summary>
    public class TokenWebRequest : RavelWebRequest {

        /// <summary>
        /// Constructor for adding token (Playerprefs)
        /// </summary>
        /// <param name="method">Method of requesting (Get, Post, Put, Etc).</param>
        /// <param name="api">Api addition part, added between base url and postfix of the whole url.</param>
        public TokenWebRequest(Method method, string api) : base(method, api) {
            AddToken();
        }

        public TokenWebRequest(string api, WWWForm form) : base(api, form) {
            AddToken();
        }
        
        public static bool HasToken() {
            if (!PlayerPrefs.HasKey(LoginRequest.SYSTEMS_TOKEN_KEY)) {
                return false;
            }
            
            LoginRequest.SimpleToken token =
                JsonUtility.FromJson<LoginRequest.SimpleToken>(PlayerPrefs.GetString(LoginRequest.SYSTEMS_TOKEN_KEY));
            
            if (token == null /*|| token.GetExpiary() < DateTime.Now*/) {
                return false;
            }

            return true;
        }

        private void AddToken() {
            string token = GetToken();
            
            if (string.IsNullOrEmpty(token)) {
                _error = InternalError.MissingToken;
                return;
            }
            
            AddHeader("Authorization", "Bearer " + token);
        }

        /// <summary>
        /// Gets the accesstoken string from the cached token in the playerpreferences.
        /// </summary>
        public static string GetToken()
        {
            LoginRequest.SimpleToken token =
                JsonUtility.FromJson<LoginRequest.SimpleToken>(PlayerPrefs.GetString(LoginRequest.SYSTEMS_TOKEN_KEY));
            
            if (token == null /*|| token.GetExpiary() < DateTime.Now*/) {
                DataServices.Login.Logout();
            }
            
            return token.token;
        }
    }
}