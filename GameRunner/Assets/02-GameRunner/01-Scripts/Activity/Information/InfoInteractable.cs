using Cohort.GameRunner.Interaction;
using Cohort.GameRunner.Players;
using UnityEngine;

public class InfoInteractable : Interactable {
    [SerializeField] private InfoDescription description;
    [SerializeField] private ObjIndicator _indicator;
    
    [SerializeField] private float _indicatorRadius = 5f;
    private bool _inIndicatorRange;
    
    public override void OnInteract() {
        if (!Value) {
            Activate();
        }
    }

    protected override void ActivateLocal() {
        InfoManager.Instance.ActivateInfo(description, this);
    }

    protected override void DeactivateLocal() { }
    
    public override bool CheckInRange() {
        _inIndicatorRange = (transform.position - Player.Local.transform.position).magnitude <= _indicatorRadius;
        
        if (_inIndicatorRange) {
            bool inRange = base.CheckInRange();
            
            _indicator.SetActive(!inRange);
            return inRange;
        }
        else {
            
            _indicator.SetActive(false);
            return false;
        }
    }
    
    #if UNITY_EDITOR
    public override void OnDrawGizmosSelected() {
        Color col = Gizmos.color;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _indicatorRadius);

        Gizmos.color = col;
        base.OnDrawGizmosSelected();
    }
    
    public override void OnValidate() {
        base.OnValidate();

        if (_networked) {
            _networked = false;
            
            Debug.LogWarning("Infopoints cannot be networked");
        }
    }
    #endif
}
