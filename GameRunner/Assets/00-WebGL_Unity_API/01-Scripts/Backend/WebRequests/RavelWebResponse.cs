using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Cohort.Networking {
    /// <summary>
    /// Base class for response data. 
    /// </summary>
    public class RavelWebResponse {
        public long ResultCode {
            get { return _result; }
        }

        /// <summary>
        /// Check if result code is 2xx
        /// </summary>
        public bool Success {
            get { return _result is >= 200 and < 300; }
        }

        /// <summary>
        /// Error class containing error message and response code
        /// </summary>
        public WebError Error {
            get { return _error; }
        }
        private WebError _error;

        /// <summary>
        /// Downloadhandler.text response, i.e. data in text format
        /// </summary>
        public string DataString {
            get { return _dataString; }
        }

        private string _dataString;

        /// <summary>
        /// Downloadhandler.bytes response, i.e. data in byte[] format
        /// </summary>
        public byte[] DataByte {
            get { return _dataByte; }
        }

        private byte[] _dataByte;

        protected long _result;

        private Sprite _sprite;
        private AssetBundle _assetBundle;

        /// <summary>
        /// Create response once call has been made.
        /// </summary>
        /// <param name="call">Finished webrequest call</param>
        public RavelWebResponse(RavelWebRequest call)
        {
            _result = call.Request.responseCode;
            if(call.Request.result != UnityWebRequest.Result.Success) {
                try {
                    _error = JsonUtility.FromJson<WebError>(call.Request.downloadHandler.text);
                    _error.code = (Code)call.Request.responseCode;
                }
                catch {
                    _error = new WebError((Code)call.Request.responseCode, call.Request.error);
                }

                if (Success) {
                    _result = (long) Code.Unknown;
                    _error.code = Code.Unknown;
                }
                _error.message += $"\n({call.Request.url})";
            }

            if (Success)
            {
                if (call.CallMethod == RavelWebRequest.Method.GetSprite) {
                    Texture2D tex = ((DownloadHandlerTexture) call.Request.downloadHandler).texture;
                    if (tex != null)
                    {
                        _sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
                    }
                    else {
                        _error = new WebError( Code.ResponseParseError, "Could not convert data into Texture 2D");
                        _result = (long) Code.ResponseParseError;
                    }
                } else if (call.CallMethod == RavelWebRequest.Method.GetAssetBundle) {
                    _assetBundle = ((DownloadHandlerAssetBundle) call.Request.downloadHandler).assetBundle;
                    if (_assetBundle == null) {
                        _error = new WebError( Code.ResponseParseError, "Could not get asset bundle from web request");
                        _result = (long) Code.ResponseParseError;    
                    }
                }
                else
                {
                    if (call.Request.downloadHandler != null) {
                        _dataByte = call.Request.downloadHandler.data;
                        _dataString = call.Request.downloadHandler.text;
                    }
                }
            }
            
            call.DisposeData();
        }

        public virtual bool TryGetSprite(out Sprite spr)
        {
            spr = _sprite;
            return _sprite != null;
        }

        public virtual bool TryGetAssetBundle(out AssetBundle assetBundle)
        {
            assetBundle = _assetBundle;
            return _assetBundle != null;
        }

        /// <summary>
        /// Convert message from backend to data container class for usage.
        /// </summary>
        /// <param name="data">variable for data response.</param>
        /// <returns>true/false data set.</returns>
        public virtual bool TryGetData<T>(out T data) {
            if(_result == (long) Code.Success) {
                try {
                    data = JsonUtility.FromJson<T>(DataString);
                    return true;
                }
                catch (Exception e) {
                    _error = new WebError(Code.ResponseParseError, $"(JSON parse error): {e.Message}");
                    _result = (long)_error.code;

                    //empty instance of 'T'.
                    data = default(T); //null
                    return false;
                }
            } else {
                Debug.LogError($"{_result}: {(Code) _result}");
                //empty instance of 'T'.
                data = default(T);
                return false;
            }
        }

        /// <summary>
        /// Return the non-JSON version of the response.
        /// </summary>
        /// <param name="data">string to contain data.</param>
        /// <returns>true/false is error.</returns>
        public virtual bool TryGetData(out string data) {
            data = DataString;
            if(Success) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get a collection of data out of the given webresponse. Uses the proxycollection class to wrap the Json
        /// data into a simple array container, so the Unity parser can parse the data.
        /// </summary>
        /// <param name="data">out variable containing the array of data that needs to be retrieved from the response.</param>
        /// <param name="collectionFieldName">Name under which the collection of proxy items is saved in the json data
        /// (so it can be parsed into this class).</param>
        /// <typeparam name="T">type of the data that is assumed to be in the servers response.</typeparam>
        public virtual bool TryGetCollection<T>(out ProxyCollection<T> data, string collectionFieldName = "") {
            data = null;
            if (Success)
            {
                try {
                    data = ProxyCollection<T>.Get(DataString, collectionFieldName);
                    return true;
                } catch(Exception e) {
                    _error = new WebError(Code.ResponseParseError, $"Faultive JSON: {e.Message}.");
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Return codes that van be send back from the server and their identifiers that match the codes.  
        /// </summary>
        public enum Code {
            ResponseParseError = -2,
            Unknown = -1,
            
            Success = 200,
            
            CannotVerify = 400,
            Unauthorized = 401,
            NotFound = 404,
            AlreadyExists = 406,
            
            UserAlreadyExists = 500,
            NotImplemented = 501,
            UserDoesNotExist = 509,
        }

        /// <summary>
        /// Simple proxy struct for errors returned from the webserver. Contains the code and the message.
        /// </summary>
        [Serializable]
        public class WebError
        {
            public string FullMessage {
                get { return $"({(int)code}): {message}"; }
            }
            
            public Code code;
            public string message;

            public WebError()
            {
                code = Code.Unknown;
            }

            public WebError(Code code, string message)
            {
                this.code = code;
                this.message = message;
            }
        }
    }
}