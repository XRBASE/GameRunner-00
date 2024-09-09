using System.Collections;
using System.Runtime.InteropServices;
using Cohort.Config;
using Cohort.Ravel.Networking.Authorization;
using Cohort.Ravel.Patterns;
using MathBuddy.Strings;
using UnityEngine;

//before login and most other things
[DefaultExecutionOrder(-99)]
public class ServerHandle : Singleton<ServerHandle>
{
    //Dll import methods are described in Plugins>WebLG>UnityWebGL.jslib
    //these are front-end methods that are called through Unity.
    
    /// <summary>
    /// Ping front-end back as response on sent ping.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void UnityPong();
    [DllImport("__Internal")]
    private static extern void UnityProgress(float progression);
    [DllImport("__Internal")]
    private static extern void UnityLoaded(bool complete);
    [DllImport("__Internal")]
    private static extern void OnAvatarChanged(string uuid, string avatarUrl);
    
    /// <summary>
    /// Ping front-end to jump to a space with given id.
    /// </summary>
    /// <param name="id"></param>
    [DllImport("__Internal")]
    private static extern void JumpSpace(string id);

    /// <summary>
    /// Is the application controlled by the server through this script?
    /// </summary>
    public bool ServerControlled { get; private set; }

    [SerializeField] private bool _testData = false;

    [SerializeField] private string _testInput =
        "{\"token\":\"eyJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhaS5zcGFjZXNoaWZ0LnBsYXRmb3JtLmNlbnRhdXIiLCJzdWIiOiJydXRnZXJ2ZDIxNyIsImV4cCI6MTcxMzM0NjY3MiwiaWF0IjoxNzEzMjYwMjcyLCJyb2xlcyI6WyJBRE1JTiIsIkdVRVNUIiwiTUFOQUdFUiIsIlNVUEVSIiwiVVNFUiJdfQ.fSKZtqa3yN_bZTVubN3Mj5UXuoRVld_pvFdQsL70qhM\",\"baseUrl\":\"https://dev.spaceshift.ai\",\"roomId\":\"58d6a142-e326-4d40-859a-3fa0d57540e2\",\"spaceId\":\"8d3d6c74-a2ce-4585-acad-b321ae6596bd\"}";
    
    protected override void Awake()
    { 
        //skip base and do singleton yourself, because of don't destroy on load
        if (Instance != null) {
            DestroyImmediate(this.gameObject);
            return;
            
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void OnDestroy() {
        AppConfig.Config.OverrideAppID("");
        AppConfig.Config.OverrideBaseUrl("");
    }

    protected void Start() {
#if (UNITY_WEBGL && !UNITY_EDITOR)
        //disable test for web builds
        _testData = false;
#endif
        if (_testData) {
            UnityPing();
            
            InitData(_testInput);
        }
    }

    /// <summary>
    /// Called by server to check whether unity has started already. Response is a ping back.
    /// </summary>
    public void UnityPing() {
        Debug.Log("UnityPing");
        if (!ServerControlled) {
            Debug.Log("ServerControlled");
            ServerControlled = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityPong();
#endif
        }
    }

    private string _initData;
    
    private string _baseUrl;

    private LoginRequest.ServerToken _token;
    
    private string _roomId;
    private bool _dataValid;
    
    public void InitData(string data) {
        _initData = data;
        Debug.Log($"Running with serverdata:\n{data}");

        StartCoroutine(Connect());
    }

    private IEnumerator Connect() {
        _dataValid = true;
        
        _baseUrl = StringExtentions.PickFromJson<string>("baseUrl", _initData) + "/";
        _dataValid = !string.IsNullOrEmpty(_baseUrl);
        AppConfig.Config.OverrideBaseUrl(_baseUrl);
        
        _token = JsonUtility.FromJson<LoginRequest.ServerToken>(_initData);
        if (_dataValid)
            _dataValid = !string.IsNullOrEmpty(_token.token);
        DataServices.Login.OnSystemsTokenRecieved(_token);
        
        _roomId = StringExtentions.PickFromJson<string>("roomId", _initData);
        if (_dataValid)
            _dataValid = !string.IsNullOrEmpty(_roomId);
        
        if (!_dataValid) {
            Debug.LogError($"Stopping logging because of data error:" +
                             $"roomid: {_roomId}, baseURL: {_baseUrl}, token: {_token.token}");
            yield break;
        }
        
        DataServices.Photon.ConnectToPhotonRoom(_roomId, null, OnConnectionError);
    }

    private void OnConnectionError(string error) {
        Debug.LogError($"Serverhandle connection error: {error}");
    }

    public void UpdateLocalUser() {
        DataServices.Users.UpdateLocalUser();
    }
    
    public void OnAvatarUpdate(string uuid, string avatarUrl) {
        Debug.Log($"on avatar changed call to server");
        
        if (ServerControlled && !_testData) {
            OnAvatarChanged(uuid, avatarUrl);
        }
    }

    public void SetKeyboardCapture(string parse)
    {
        bool capture = bool.Parse(parse);
#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log($"setting keyboard capture ({capture})");
        WebGLInput.captureAllKeyboardInput = capture;
#else
        Debug.Log($"Server call SetKeybaordCapture ({capture})!");
#endif
    }
    
    public void SetCursorCapture(string parse)
    {
        bool capture = bool.Parse(parse);
        //API_TODO: input management
        //InputManager.Instance.Cursor.ControlEnabled = capture;
        Debug.Log($"Server call SetCursorCapture ({capture})!");
    }

    public void ActivateShift(string parse) {
        bool active = bool.Parse(parse);
        
        //API_TODO: shifting effect
        //CameraState.Instance.ActivateShift(active);
    }

#region PORTALS
    /// <summary>
    /// Called when the user enters a portal and ServerControlled is true.
    /// </summary>
    /// <param name="spaceId">Uuid of destination space</param>
    public void JumpToSpace(string spaceId) {
#if UNITY_WEBGL && !UNITY_EDITOR
        JumpSpace(spaceId);
#else
        Debug.Log("Server call enter portal in editor!");
#endif
    }
    
#endregion

#region LOADING

    private void OnLoading(int percentage, string message) {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityProgress(percentage);
#else
        
#endif
        Debug.Log($"Loading {message} ({percentage})");
    }

    private void OnLoadingFinished() {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityLoaded(true);
#else
        
#endif
        Debug.Log($"Loading finished");
    }
#endregion
}


