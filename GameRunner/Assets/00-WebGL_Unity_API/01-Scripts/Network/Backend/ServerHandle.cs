using Cohort.Networking.Authorization;
using Cohort.GameRunner.Loading;
using MathBuddy.Strings;
using Cohort.Patterns;
using Cohort.Config;

using System.Runtime.InteropServices;
using System.Collections;
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
    private static extern void UnityProgress(int percentage);
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

    public ActivityDescription activityDef;
    [SerializeField] private string _testInput =
        "{\"token\":\"eyJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJhaS5zcGFjZXNoaWZ0LnBsYXRmb3JtLmNlbnRhdXIiLCJzdWIiOiJydXRnZXJ2ZDIxNyIsImV4cCI6MTcxMzM0NjY3MiwiaWF0IjoxNzEzMjYwMjcyLCJyb2xlcyI6WyJBRE1JTiIsIkdVRVNUIiwiTUFOQUdFUiIsIlNVUEVSIiwiVVNFUiJdfQ.fSKZtqa3yN_bZTVubN3Mj5UXuoRVld_pvFdQsL70qhM\",\"baseUrl\":\"https://dev.spaceshift.ai\",\"roomId\":\"58d6a142-e326-4d40-859a-3fa0d57540e2\",\"spaceId\":\"8d3d6c74-a2ce-4585-acad-b321ae6596bd\"}";
    
    
    private void OnDestroy() {
        AppConfig.Config.OverrideAppID("");
        AppConfig.Config.OverrideBaseUrl("");
    }

    protected void Start() {
#if (UNITY_WEBGL && !UNITY_EDITOR)
        //disable test for web builds
        _testData = false;
        
        LoadingManager.Instance.onLoadingChanged += OnLoading;
        LoadingManager.Instance.onLoadingFinished += OnLoadingFinished;
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
        if (!ServerControlled) {
            ServerControlled = true;

#if UNITY_WEBGL && !UNITY_EDITOR
            UnityPong();
#endif
        }
    }

    private string _initData;
    
    private string _baseUrl;

    private LoginRequest.SimpleToken _token;
    
    private string _sessionId;
    private bool _dataValid;
    
    public void InitData(string data) {
        _initData = data;
        Debug.Log($"Running with serverdata:\n{data}");

        StartCoroutine(Connect());
    }

    private IEnumerator Connect() {
        _dataValid = true;
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.ConnectToPhoton].Start();
        
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.ConnectToPhoton].Increment("loading network data");
        _baseUrl = StringExtentions.PickFromJson<string>("baseUrl", _initData) + "/";
        _dataValid = !string.IsNullOrEmpty(_baseUrl);
        AppConfig.Config.OverrideBaseUrl(_baseUrl);
        
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.ConnectToPhoton].Increment("Logging in user");
        _token = JsonUtility.FromJson<LoginRequest.SimpleToken>(_initData);
        if (_dataValid)
            _dataValid = !string.IsNullOrEmpty(_token.token);
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.RetrieveUserData].Start();
        DataServices.Login.OnSystemsTokenRecieved(_token);
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.RetrieveUserData].Increment("Playerdata recieved");
        
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.RetrieveUserData].Finish();
        
        
        _sessionId = StringExtentions.PickFromJson<string>("sessionId", _initData);
        if (_dataValid)
            _dataValid = !string.IsNullOrEmpty(_sessionId);
        
        if (!_dataValid) {
            Debug.LogError($"Stopping logging because of data error:" +
                             $"sessionId: {_sessionId}, baseURL: {_baseUrl}, token: {_token.token}");
            yield break;
        }
        
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.ConnectToPhoton].Increment("Connecting networking client");
        DataServices.Photon.ConnectToPhotonRoom(_sessionId, OnConnected, OnConnectionError);
    }

    private void OnConnected() {
        LoadingManager.Instance[LoadPhase.Lobby, LoadType.ConnectToPhoton].Finish();
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
        //TODO_COHORT: input management
        //InputManager.Instance.Cursor.ControlEnabled = capture;
        Debug.Log($"Server call SetCursorCapture ({capture})!");
    }

    public void ActivateShift(string parse) {
        bool active = bool.Parse(parse);
        
        //TODO_COHORT: shifting effect
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

    private void OnLoading(float progression, string message) {
        int percentage = Mathf.RoundToInt(progression * 100);
        
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityProgress(percentage);
#endif
        Debug.Log($"Loading {message} ({percentage})");
    }

    private void OnLoadingFinished() {
#if UNITY_WEBGL && !UNITY_EDITOR
        UnityLoaded(true);
#endif
        Debug.Log($"Loading finished");
    }
#endregion
}