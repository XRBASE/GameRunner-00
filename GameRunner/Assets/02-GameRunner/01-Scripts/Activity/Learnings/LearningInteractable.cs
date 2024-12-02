using System;
using Cohort.CustomAttributes;
using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.PlayerLoop;

[DefaultExecutionOrder(0)] //Before learningManager
public class LearningInteractable : Interactable {
    
    public string LocationDescription { get { return _locationDescription; } }
    public bool HasLearning {
        get { return _hasLearning;}
        private set {
            interactable = value;
            _hasLearning = value;
        }
    }
    [ReadOnly, SerializeField] private bool _hasLearning;
    
    [SerializeField] private string _locationDescription = "At position";
    [SerializeField] private ObjIndicator _indicator;
    
    private int _actor = -1;
    private LearningDescription _learning;

    private void Awake() {
        interactable = false;
    }

    protected override void Start() {
        _networked = _networked && LearningManager.Instance.LearningsNetworked;
        
        base.Start();
        
        if (_networked) {
            Network.Local.Callbacks.onPlayerLeftRoom += OnPlayerLeftRoom;
        }
    }

    public override void SetInRange(bool value) {
        base.SetInRange(value);
        
        _indicator.SetActive(!InRange && HasLearning);
    }

    public override void OnInteract() {
        if (!HasLearning) {
            return;
        }
        
        Activate();
    }

    protected override void Activate(Hashtable changes, Hashtable expected = null) {
        if (!_networked) {
            base.Activate(null, null);
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

        if (!_networked || _actor == Player.Local.ActorNumber) {
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
        SetLearningLocal(-1);
    }

    public void SetLearning(LearningDescription learning = null) {
        if (!_networked) {
            if (learning == null)
                SetLearningLocal(-1);
            else
                SetLearningLocal(learning.index);
            
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
    
    public void SetLearningLocal(int index) {
        HasLearning = index >= 0;
        
        if (HasLearning) {
            _learning = LearningManager.Instance[index];
            
            _indicator.SetActive(!InRange && HasLearning);
            LearningManager.Instance.SetLearningLog(_learning, this);
            return;
        }
        
        _indicator.SetActive(false);
        
        if (_learning != null) {
            LearningManager.Instance.RemoveLearningLog(_learning);
            _learning = null;
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
