using System.Collections;
using System.Collections.Generic;
using Cohort.Patterns;
using Photon.Realtime;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//called pretty much before anything, but after the default early scripts. This is to ensure that network.service is called
//before most other scripts use the fixed update to retrieve the net data.
[DefaultExecutionOrder(-99)]
public class Networker : Singleton<Networker>
{
    private const string LOCAL_ID = "local";
    private const float NETWORK_UPDATE_INTERVAL = 0.1f;
    
    /// <summary>
    /// True if class has been cleared and networks do not exist anymore.
    /// </summary>
    public bool Disposed { get; private set; }
    public Dictionary<string, Network> Networks { get; private set; }
    
    private List<string> _toRemoveIds;
    private bool _networkUpdates;
    
    protected override void Awake()
    {
        base.Awake();
        Networks = new Dictionary<string, Network>();
        _toRemoveIds = new List<string>();

        _networkUpdates = true;
        StartCoroutine(NetworkServiceUpdate());
        
        AddNetwork(LOCAL_ID, true);
    }

    protected void OnDestroy()
    {
        _networkUpdates = false;
        StopAllCoroutines();
        
        foreach (var kv_network in Networks) {
            kv_network.Value.Dispose();
        }
        
        //clear remove, all networks are disconnected anyway
        _toRemoveIds.Clear();
        Networks.Clear();
        Disposed = true;
    }

    public Network AddNetwork(string id, bool isLocal = false)
    {
        //only local channel auto reconnects and includes chat
        Networks.Add(id, new Network(isLocal, isLocal && Network.AUTO_RECONNECT));
        //this callback is never unsubbed, but the network is disposed, so it is the last time disconnect is called ever.
        Networks[id].Callbacks.onDisconnect += (cause) => RemoveNetwork(id);
        return Networks[id];
    }

    public void RemoveNetwork(string id)
    {
        //local is only disposed in the onDestroy call to preserve ze data
        if (id == LOCAL_ID || Networks[id].Reconnecting) {
            //if the network reconnects it is not removed
            return;
        }
        
        Networks[id].Dispose();
        _toRemoveIds.Add(id);
    }
    
    private IEnumerator NetworkServiceUpdate() {
        while (_networkUpdates) {
            foreach (var kv_network in Networks)
            {
                if (kv_network.Value.Client.IsConnected) {
                    kv_network.Value.Service();
                }
            }

            //prevent enumerator changes from happening in the loop above this.
            foreach (var id in _toRemoveIds) {
                Networks.Remove(id);
            }
            _toRemoveIds.Clear();

            yield return new WaitForSeconds(NETWORK_UPDATE_INTERVAL);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(Networker))]
    private class NetworkerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect")) {
                Network.Local.ConnectToNetwork(Network.Local.Client.NickName);
            }
            if (GUILayout.Button("Disconnect")) {
                Network.Local.Disconnect();
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Simulate timeout (on local)")) {
                Network.Local.Disconnect(DisconnectCause.ClientTimeout);
            }
        }
    } 
#endif
}
