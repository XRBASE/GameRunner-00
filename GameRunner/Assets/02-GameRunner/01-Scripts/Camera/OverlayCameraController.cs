using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class OverlayCameraController : MonoBehaviour {
    private UniversalAdditionalCameraData _mainData;
    private Camera _main;

    private Camera _cam;
    private bool _subscribed;

    private void Start() {
        _main = Camera.main;
        _mainData = _main.GetComponent<UniversalAdditionalCameraData>();
        
        _cam = GetComponent<Camera>();
        
        _mainData.cameraStack.Add(_cam);
        _subscribed = true;
    }

    private void OnDestroy() {
        if (!_subscribed) {
            _mainData.cameraStack.Remove(_cam);
        }
    }
}
