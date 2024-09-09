using Cohort.Config;
using Cohort.Ravel.PhotonNetworking.Callbacks;
using Cohort.Ravel.PhotonNetworking.Rooms;
using Cohort.Ravel.Tools.Timers;
using Photon.Realtime;
using UnityEngine;

public class Network {
    public const bool AUTO_RECONNECT = true;
    public const int MAX_RECONNECT_TRIES = 5;
    public const float RECONNECT_DURATION = 1f;
    public const string REGION = "EU";
    
    public static Network Local { get; private set; }
    public PhotonClient Client { get; private set; }
    public RealtimeCallbackHandle Callbacks { get; }
    public RoomManager RoomManager { get; private set; }
    public bool Reconnecting { get; private set; }

    private bool _reconnectOnDisconnectOverride = false;
    private bool _clientDataReconnect = false;

    private string _appid;
    
    private Timer _reconnectTimer;
    private bool _autoReconnect;
    private int _reconectCounter;
    
    private bool _disposed;
    private bool _isLocal;
    
    public Network(bool isLocal, bool autoReconnect = true)
    {
        _isLocal = isLocal;
        if (isLocal) {
            Local = this;
        }
        
        Client = new PhotonClient();
        
        _autoReconnect = autoReconnect;
        
        Callbacks = new RealtimeCallbackHandle(this);
        Callbacks.onConnect += OnConnected;
        Callbacks.onDisconnect += OnDisconnected;

        RoomManager = new RoomManager(Client, Callbacks);
        _reconectCounter = MAX_RECONNECT_TRIES;
    }

    ~Network()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        if (_disposed)
            return;
        RoomManager.Dispose();

        Callbacks.Dispose();
        
        //prevent anyone from accessing client,
        lock (Client) {
            //disconnect and directly send the disconnect to the server
            Disconnect();
            Client.Service();
                
            //remove client reference so it cannot be reconnected outside of the lock
            Client = null;
        }

        _disposed = true;
    }
    
    /// <summary>
    /// Service sends and retrieves data from/to the photon network.
    /// </summary>
    public void Service()
    {
        if(Client.IsConnected)
            Client.Service();
    }
    
    public void ConnectToNetwork(string userName)
    {
        
        if (string.IsNullOrEmpty(_appid)) {
            Debug.Log($"Missing appid, could not connect to Photon!");
            return;
        }
        
        Client.AppId = _appid;
        Client.AppVersion = AppConfig.GetMajorVersionNumber();
        
        Networker.Instance.StartCoroutine(Callbacks.WaitForConnectedAndReady());
        if (Client.IsConnected) {
            Debug.LogWarning("Tried connecting, but the client is already connected!");
            return;
        }
        
        //TODO: unique nicknames in case Johan Johansen comes back again.
        Client.NickName = userName;
        
        Debug.Log($"Connecting to master ({REGION}).");
        Client.ConnectToRegionMaster(REGION);
    }

    public void UpdateAppId(string newId)
    {
        if (Client.IsConnected) {
            _clientDataReconnect = true;
            if (Client.InRoom) {
                RoomManager.LeaveRoom();
            }
        }

        if (_appid != newId) {
            _appid = newId;
            Reconnect(true);
        }
    }
    
    public void Reconnect()
    {
        Reconnect(false);
    }
    
    public void Reconnect(bool updateClientData)
    {
        if (Client.IsConnected) {
            _reconnectOnDisconnectOverride = true;
            Debug.Log("reconnecting...");
            Disconnect(DisconnectCause.None);
        }
        else {
            //reset userid
            //Client.UserId = "";
            //if user previously was in a room, reconnect to that room
            if (!updateClientData) {
                Callbacks.onConnectedAndReady += RoomManager.OnReconnect;
            }

            ConnectToNetwork(Client.NickName);
        }
    }

    public void OnConnected()
    {
        Reconnecting = false;
        _reconectCounter = MAX_RECONNECT_TRIES;
    }

    public void Disconnect(DisconnectCause cause = DisconnectCause.DisconnectByClientLogic)
    {
        if (Client != null && Client.IsConnected) {
            Client.Disconnect(cause);    
        }
    }

    private void OnDisconnected(DisconnectCause cause)
    {
        if (_clientDataReconnect) {
            Reconnecting = true;
            //if client data is updated when reconnecting, skip the timer.
            //set internal var to false after reconnect is called.
            _clientDataReconnect = false;
            Reconnect(true);
            return;
        }
        
        if (_reconnectOnDisconnectOverride) {
            //boolean is reset after use
            _reconnectOnDisconnectOverride = false;
        } else if (!_autoReconnect) {
            //return if no reconnect on disconnect option is available
            return;
        }

        if (cause != DisconnectCause.None && cause != DisconnectCause.DisconnectByClientLogic) {
            Debug.LogError($"Realtime disconnected: {cause}");
        }
        
        if (_reconectCounter <= 0) {
            //reconnecting fialed, set to false and invoke disconnect
            Reconnecting = false;
            _reconnectTimer.Remove();
            _reconnectTimer = null;
            
            Callbacks.onReconnectFailed?.Invoke(cause);
            return;
        }
        
        _reconectCounter--;
        
        switch (cause) {
            case DisconnectCause.None:
            case DisconnectCause.Exception:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.OperationNotAllowedInCurrentState:
                Reconnecting = true;
                if (_reconnectTimer == null) {
                    //create new timer if no one exists
                    _reconnectTimer = new Timer(RECONNECT_DURATION, true, Reconnect);
                } else if (_reconnectTimer.HasFinished) {
                    //reset timer if already finished
                    _reconnectTimer.Reset();
                    _reconnectTimer.Start();
                }
                break;
        }
    }
}
