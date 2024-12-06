using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CameraOverlayCanvas : MonoBehaviour {
    private Canvas _canvas;
    private Camera _mainCam;

    private void Start() {
        _mainCam = Camera.main;

        _canvas = GetComponent<Canvas>();
        
        _canvas.worldCamera = _mainCam;
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }
}
