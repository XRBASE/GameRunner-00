using Cohort.GameRunner.LocoMovement;
using UnityEngine;
using MathBuddy;

/// <summary>
/// Oh step-raycaster wat are you doing UwU.
/// </summary>
public class StepRaycaster : MonoBehaviour {
    //max height any traversible step could be
    public const float UPP_BOUND = 0.3f;
    public const float LOW_BOUND = 0.1f;
    public const float ANGLE = 10f;
    //distance in front of the player where the player's body will move upwards
    public const float FWD = 0.5f;

    private Rigidbody _rb;
    private Locomotion _lm;
    private LayerMask _mask;
    
    public void Initialize(Rigidbody rb, Locomotion lm) {
        Initialize(rb, lm, ~(LayerMask.GetMask("Player")));
    }

    public void Initialize(Rigidbody rb, Locomotion lm, LayerMask mask) {
        _rb = rb;
        _lm = lm;
        
        _mask = mask;
        transform.SetParent(_rb.transform, false);
        transform.position = Vector3.up * UPP_BOUND
                             + transform.InverseTransformDirection(Vector3.forward * FWD);
    }

    private void Update() {
        if (_lm.GroundCheck.Grounded && _rb.transform.InverseTransformDirection(_rb.velocity).z > 0 && 
            RaycastStep(UPP_BOUND, out float d) && d > LOW_BOUND) {
            _rb.MovePosition(_rb.transform.position + new Vector3(0,d, 0));
        }
    }

    public bool RaycastStep(float maxDist, out float dist) {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, maxDist, _mask, QueryTriggerInteraction.Ignore)) {
            dist = Mathf.Abs(_rb.transform.position.y - hit.point.y);
            
            return dist > FloatingPoints.LABDA && Vector3.Angle(hit.normal, Vector3.up) <= ANGLE;
        }

        dist = Mathf.Infinity;
        return false;
    }
}
