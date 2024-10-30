using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using Cohort.UI.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

public class LearningInteractable : Interactable {
    public bool HasLearning { get; private set; }
    
    [SerializeField] private string _locationDescription = "At position";
    [SerializeField] private ObjIndicator _indicator;
    
    private int _actor;
    private LearningLogEntry _logEntry;
    private LearningDescription _learning;

    protected override void Start() {
        base.Start();

        _networked = _networked && LearningManager.Instance.LearningsNetworked;

        if (_networked) {
            Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
        }
    }
    
    public override void OnInteract() {
        if (!HasLearning) {
            return;
        }
        
        Activate();
    }

    protected override void Activate(Hashtable changes, Hashtable expected = null) {
        if (!_networked) {
            base.Activate(changes, expected);
            return;
        }
        
        if (changes == null)
            changes = new Hashtable();
        if (expected == null)
            expected = new Hashtable();
        
        changes.Add(GetActorKey(), Player.Local.ActorNumber);
        expected.Add(GetInteractableKey(), false);
        
        base.Activate(changes, expected);
    }
    
    protected override void ActivateLocal() {
        _indicator.SetActive(false);

        if (_actor == Player.Local.ActorNumber) {
            LearningManager.Instance.OnLearningStart(_learning, this);
        }
    }

    protected override void Deactivate(Hashtable changes, Hashtable expected = null) {
        if (!_networked) {
            base.Deactivate(changes, expected);
            return;
        }
        
        if (changes == null) 
            changes = new Hashtable();
        
        changes.Add(GetActorKey(), null);
        changes.Add(GetLearningKey(), -1);
        
        //if there is an actor this item can only be deactivated by that actor.
        if (_actor != -1) {
            if (expected == null)
                expected = new Hashtable();

            expected.Add(GetActorKey(), Player.Local.ActorNumber);
        }
        base.Deactivate(changes, expected);
    }

    protected override void DeactivateLocal() {
        _learning = null;
    }

    public void SetLearning(LearningDescription learning = null) {
        if (!_networked) {
            SetLearningLocal(learning?.index ?? -1);
            return;
        }
        
        Hashtable changes = new Hashtable();
        if (learning != null) {
            changes.Add(GetLearningKey(), learning.index);
        }
        else {
            changes.Add(GetLearningKey(), -1);
        }

        Network.Local.Client.CurrentRoom.SetCustomProperties(changes);
    }
    
    private void SetLearningLocal(int index) {
        HasLearning = index >= 0;
        
        if (HasLearning) {
            _learning = LearningManager.Instance[index];
            
            _indicator.SetActive(true);
            //TODO: possibly these two parts can be split, so that the log is just initialized and linked to the on mini game finished call?
            
            _logEntry = UILocator.Get<LearningLogUI>().CreateLogEntry(_learning.actionDescription, _locationDescription);
            return;
        }
        
        _indicator.SetActive(false);
        if (_logEntry != null) {
            if (_actor == Player.Local.ActorNumber) {
                _logEntry.CheckLogItem(_learning.state);
            }
            else {
                //task has not failed, but someone else solved the learning.
                _logEntry.CheckLogItem(LearningDescription.State.Failed);
            }

            _learning = null;
            _logEntry = null;
        }
    }
    
    private void OnPlayerLeftRoom(Photon.Realtime.Player obj) {
        if (_actor == obj.ActorNumber) {
            _actor = -1;
            Deactivate();
        }
    }

    protected override void OnJoinedRoom() {
        base.OnJoinedRoom();

        if (Value && !PlayerManager.Instance.ActorNumberExists(_actor)) {
            _actor = -1;
            Deactivate();
        }
    }

    protected override void OnPropertiesChanged(Hashtable changes) {
        string key = GetLearningKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                SetLearningLocal(-1);
            }
            else {
                SetLearningLocal((int)changes[key]);
            }
        }

        key = GetActorKey();
        if (changes.ContainsKey(key)) {
            if (changes[key] == null) {
                _actor = -1;
            }
            else {
                _actor = (int)changes[key];
            }
        }
        
        base.OnPropertiesChanged(changes);
    }
    
    private string GetActorKey() {
        return Keys.Concatenate(
            Keys.Concatenate(Keys.Room.Learning, Keys.Learning.Actor), 
            Identifier.ToString());
    }

    private string GetLearningKey() {
        return Keys.Concatenate(
            Keys.Concatenate(Keys.Room.Learning, Keys.Learning.Index), 
            Identifier.ToString());
    }
}
