using System.Collections.Generic;
using System.IO;
using ExitGames.Client.Photon;
using UnityEngine;

public class PhotonDataDebugger : MonoBehaviour
{
#if UNITY_EDITOR
    private const string RAP_PATH = "/Rapports/";
    
    private Dictionary<string, long> _playerDicSize = new Dictionary<string, long>();
    private Dictionary<string, long> _roomDicSize = new Dictionary<string, long>();
    
    private List<string> _roomRapport = new List<string>();
    private List<string> _playerRapport = new List<string>();

    private float _enterRoomT;
    private bool _inRoom = false;

    public void Start(){
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onLeavingRoom += LeftRoom;
        
        Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropsChanged;
        Network.Local.Callbacks.onPlayerPropertiesChanged += OnPlayerPropertiesChanged;
    }
    
    public void OnApplicationQuit(){
        if(_inRoom)
            LeftRoom();
        
        WriteFile();
    }

    public void WriteFile(){
        int name = 0;
        string path;
        bool found = false;
        while (!found){
            if (File.Exists(Application.streamingAssetsPath + RAP_PATH + name.ToString() + ".txt")){
                name++;
            }
            else{
                found = true;
            }
        }

        path = Application.streamingAssetsPath + RAP_PATH + name.ToString() + ".txt";
        using (StreamWriter sw = new StreamWriter(path)){
            sw.WriteLine("Room data:");
            sw.WriteLine("\t Occurrences:");
            foreach (var kv_roomSize in _roomDicSize){
                sw.WriteLine($"\t\t Key: {kv_roomSize.Key}, count: {kv_roomSize.Value}");
            }

            sw.WriteLine("\t data:");
            foreach (var data in _roomRapport){
                sw.WriteLine($"\t {data}");
            }

            sw.WriteLine("Player data:");
            sw.WriteLine("\t Occurrences:");
            foreach (var kv_playerSize in _playerDicSize){
                sw.WriteLine($"\t\t Key: {kv_playerSize.Key}, count: {kv_playerSize.Value}");
            }

            sw.WriteLine("\t data:");
            foreach (var data in _playerRapport){
                sw.WriteLine($"\t {data}");
            }
        }
    }

    public void OnJoinedRoom(){
        _roomRapport.Add($"Joined room {Network.Local.Client.CurrentRoom}");
        _enterRoomT = Time.realtimeSinceStartup;
        _inRoom = true;
    }

    public void LeftRoom(){
        _roomRapport.Add($"Time in room: {Time.realtimeSinceStartup - _enterRoomT}");
        _inRoom = false;
    }
    
    public void OnRoomPropsChanged(Hashtable changes)
    {
        string data = "";
        foreach (var entry in changes) {
            data += $"\t {entry.Key}:{entry.Value}\n";
            if (!_roomDicSize.ContainsKey((string)entry.Key)){
                _roomDicSize[(string)entry.Key] = 1;
            }
            else {
                _roomDicSize[(string)entry.Key] += 1;
            }
        }
        _roomRapport.Add(data);
    }
    
    public void OnPlayerPropertiesChanged(Photon.Realtime.Player player, Hashtable changes)
    {
        string data = $"Player({player.ActorNumber}):\n";
        foreach (var entry in changes) {
            data += $"\t {entry.Key}:{entry.Value.ToString()}\n";
            
            if (!_playerDicSize.ContainsKey((string)entry.Key)){
                _playerDicSize[(string)entry.Key] = 1;
            }
            else {
                _playerDicSize[(string)entry.Key] += 1;
            }
        }
        
        _playerRapport.Add(data);
    }
#endif
}
