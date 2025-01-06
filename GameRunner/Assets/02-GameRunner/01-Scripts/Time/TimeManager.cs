using System;
using Cohort.CustomAttributes;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using UnityEngine;
using Cohort.Patterns;
using ExitGames.Client.Photon;

//After playermanager
[DefaultExecutionOrder(-1)]
public class TimeManager : Singleton<TimeManager>
{
    //if time from the master differs by this threshold, the ref time is updated to match the master's time.
    public const float ADJUSTMENT_THRESHOLD = 0.05f;
    public const float RESET_VALUE = 500f;
    
    //no worries about overflow, the sun will burn up before this float will overload
    /// <summary>
    /// Reference time that is used to sync up the timed values like audio and animation.
    /// </summary>
    public float RefTime {
        get { return _refTime; }
    }

    public bool TimeLord {
        get { return _isTimeLord; }
    }

    /// <summary>
    /// Callback that is triggered when the reference time difference becomes to big and has to be synced up again.
    /// </summary>
    public Action<float> onRefTimeChange;
    public Action onRefTimeReset;
    
    //Am I the master that tracks the official ref time and syncs my value?
    private bool _isTimeLord;
    
    //As changes to time value are pushed each fixed update, just keep this table prefixed for setting time.
    private Hashtable _timeTable;
    private string _timeKey;
    private bool _timeReset = false;
    private bool _callReset = false;
    [ReadOnly, SerializeField] private float _refTime;

    private void Start()
    {
        Network.Local.Callbacks.onJoinedRoom += OnJoinedRoom;
        Network.Local.Callbacks.onLeftRoom += OnLeftRoom;
        Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged += OnRoomPropsChanged;
        Network.Local.Callbacks.onService += UpdateNetwork;
        
        _timeKey = Keys.Get(Keys.Room.RefTime);
    }
    
    private void OnDestroy()
    {
        Network.Local.Callbacks.onJoinedRoom -= OnJoinedRoom;
        Network.Local.Callbacks.onLeftRoom -= OnLeftRoom;
        Network.Local.Callbacks.onPlayerLeftRoom -= OnPlayerLeftRoom;
        Network.Local.Callbacks.onRoomPropertiesChanged -= OnRoomPropsChanged;
        Network.Local.Callbacks.onService -= UpdateNetwork;
    }

    private void OnLeftRoom()
    {
        _isTimeLord = false;
    }

    private void Update()
    {
        _refTime += Time.deltaTime;

        //Checks if reset point has been reached and resets the time.
        if (RefTime > RESET_VALUE) {
            _refTime %= RESET_VALUE;
            //this boolean triggers the onRefTimeReset call, but this happens after the network call of photon, that's
            //why both this bool and callreset are used in succession of eachother. This ensures that all player's have recieved the 
            //time reset locally, before invoking the reset call.
            _timeReset = true;
        }
    }

    private void UpdateNetwork()
    {
        if (_timeReset) {
            _timeReset = false;
            _callReset = true;
        }
        
        if (!_isTimeLord || !Network.Local.Client.IsConnectedAndReady)
            return;
        
        //network RefTime to other players
        //add one timestep to it, as players will recieve it the next fixed update
        _timeTable[_timeKey] = RefTime + Time.fixedDeltaTime;
        Network.Local.Client.CurrentRoom.SetCustomProperties(_timeTable);
    }
    
    private void OnPlayerLeftRoom(Photon.Realtime.Player player)
    {
        //if the timelord leaces the room, all players send a request to become timelord.
        string key = Keys.Concatenate(Keys.Room.Time, Keys.Time.TimeLord);
        if (Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) && 
            (int) Network.Local.Client.CurrentRoom.CustomProperties[key] == player.ActorNumber) {
            //because the expected value is the actor nunmber of the player that has just left, only the first player
            //is made to be timelord.
            TrySetTimeLord(player.ActorNumber);
        }
    }

    private void OnJoinedRoom()
    {
        //if nothing set to timelord or the set timelord does not exist, preset value to 0.
        string key = Keys.Concatenate(Keys.Room.Time, Keys.Time.TimeLord);
        if (!Network.Local.Client.CurrentRoom.CustomProperties.ContainsKey(key) ||
            !PlayerManager.Instance.ActorNumberExists((int)Network.Local.Client.CurrentRoom.CustomProperties[key])) 
        {
            Hashtable changes = new Hashtable();
            changes.Add(key, 0);
            _refTime = 0f;
            Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
        }
        
        OnRoomPropsChanged(Network.Local.Client.CurrentRoom.CustomProperties);
    }

    private void OnRoomPropsChanged(Hashtable changes)
    {
        string key = Keys.Concatenate(Keys.Room.Time, Keys.Time.TimeLord);
        if (changes.ContainsKey(key)) {
            if (changes[key] == null || (int)changes[key] == 0){
                //no timelord, try to set timelord to me
                UpdateTime(changes);
                //RefTime = 0f;
                //onRefTimeReset?.Invoke();
                TrySetTimeLord();
            }
            else {
                int actor = (int) changes[key];
                _isTimeLord = actor == Player.Local.ActorNumber;
                if (_isTimeLord) {
                    //considering this table is changed 10 times per second, the key and table are reused, so they don't have to be declared so much. 
                    _timeTable = new Hashtable();
                    _timeTable.Add(_timeKey, RefTime);
                }
                else {
                    _timeTable = null;
                }
            }
        }

        UpdateTime(changes);
    }

    private void UpdateTime(Hashtable changes) {
        //if the user isn't the timelord, check for time updates.
        if (changes.ContainsKey(_timeKey) && changes[_timeKey] != null) {
            if (!TimeLord) {
                float t = (float)changes[_timeKey];
                if (Mathf.Abs(t - RefTime) > ADJUSTMENT_THRESHOLD) {
                    //add one fixed step as time recieved was (most likly) from the previous fixed update
                    _refTime = t;
                    onRefTimeChange?.Invoke(RefTime);
                }
            }

            if (_callReset) {
                _callReset = false;
                onRefTimeReset?.Invoke();
            }
        }
    }

    /// <summary>
    /// Try to become timelord
    /// </summary>
    /// <param name="exp">expected actor number, only override if taking over the role.</param>
    private void TrySetTimeLord(int exp = 0)
    {
        //the laws of time are MINE
        string key = Keys.Concatenate(Keys.Room.Time, Keys.Time.TimeLord);
        Hashtable changes = new Hashtable();
        changes.Add(key, Player.Local.ActorNumber);
        
        Hashtable expected = new Hashtable();
        expected.Add(key, exp);
        
        //only set value to my id, if I was the first to do so
        Network.Local.Client.CurrentRoom.SetCustomProperties(changes, expected);
    }
}
