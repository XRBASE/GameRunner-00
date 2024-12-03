using Avatar = Cohort.GameRunner.Avatars.Avatar;
using Cohort.GameRunner.Players;
using Cohort.GameRunner.Input;
using Cohort.Patterns;
using UnityEngine;

public class CameraController : Singleton<CameraController> {
    private const float MAX_ZOOM = 20f;
    private const float MIN_ZOOM = 3f;
    private const float MOVE_ELA = 0.02f;
    private const float LOOK_ELA = 0.1f;

    private Transform _follow;
    private Transform _lookAt;
    
    private float _zoom;
    private Vector3 _offset;

    protected void Start() {
        InputManager.Instance.CameraInput.zoom += OnZoom;
        _follow = Player.Local.transform;
        
        _offset = transform.position - _follow.position;
        _zoom = _offset.magnitude;
        if (_zoom > MAX_ZOOM || _zoom < MIN_ZOOM) {
            OnZoom(0f);
        }
        
        Player.Local.onAvatarImported += SetUp;
        
        enabled = false;
    }

    private void SetUp(Avatar avatar) {
        _lookAt = avatar.Head;
        
        enabled = true;
    }
    
    void OnZoom(float zoom) {
        _zoom = Mathf.Clamp(_offset.magnitude + zoom * Time.deltaTime, MIN_ZOOM, MAX_ZOOM);
        _offset = _offset.normalized * _zoom;
    }

    private void Update() {
        float t = MathBuddy.TLerp.Lerp.TLerp01(Time.deltaTime, MOVE_ELA);
        Vector3 goalPos = _follow.position + _offset;
        transform.position = Vector3.Lerp(transform.position, goalPos, t);
        
        t = MathBuddy.TLerp.Lerp.TLerp01(Time.deltaTime, LOOK_ELA);
        Quaternion goal = Quaternion.LookRotation(_lookAt.position - transform.position, _follow.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, goal, t);
    }
}