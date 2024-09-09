using UnityEngine;
using System.Runtime.InteropServices;
using Cohort.Ravel.Patterns;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SampleServerHandle : Singleton<SampleServerHandle>
{
	/// <summary>
	/// Ping front-end back as response on sent ping.
	/// </summary>
	[DllImport("__Internal")]
	private static extern void UnityPong();
	[DllImport("__Internal")]
	private static extern void UnityProgress(float progression);
	[DllImport("__Internal")]
	private static extern void UnityLoaded(bool complete);
    
	/// <summary>
	/// Ping front-end to jump to a space with given id.
	/// </summary>
	/// <param name="id"></param>
	[DllImport("__Internal")]
	private static extern void JumpSpace(string id);

	[SerializeField] private LoadingSample _loading;
	
	/// <summary>
	/// Called by server to check whether unity has started already. Response is a ping back.
	/// </summary>
	public void UnityPing() {
		Debug.Log("UnityPing");
		
#if UNITY_WEBGL && !UNITY_EDITOR
            UnityPong();
#endif
	}
	
	public void InitData(string data) {
		Debug.Log($"Running with serverdata:\n{data}");
		
		_loading.StartLoading();
	}
	
	public void SetKeyboardCapture(string parse)
	{
		bool capture = bool.Parse(parse);
#if UNITY_EDITOR
		_keycapture = capture;
#else
#if UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = capture;
#endif
#endif
		Debug.Log($"Server call SetKeyboardCapture ({capture})!");
	}
	
	public void SetCursorCapture(string parse)
	{
		bool capture = bool.Parse(parse);
#if UNITY_EDITOR
		_inShift = capture;
#endif
		Debug.Log($"SetCursorCapture ({capture})!");
	}

	public void ActivateShift(string parse) {
		bool active = bool.Parse(parse);
		Debug.Log($"Shift ({active})!");
#if UNITY_EDITOR
		_inShift = active;
#endif
		
		ShiftingSample.Instance.Shift(active);
	}
	
	public void OnLoadingChanged(string action, float dec, bool fin) {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (fin) {
            UnityLoaded(true);
        }
        else {
            UnityProgress(Mathf.Round(dec * 100));
        }
#else
		Debug.Log($"Loading changed server call {(fin? "finished" : $"loading {action}: ({Mathf.Round(dec * 100)})")}");
#endif
	}
	
#if UNITY_EDITOR
	private bool _inShift = false;
	private bool _keycapture = true;
	private bool _cursorActive = true;
	
	[CustomEditor(typeof(SampleServerHandle))]
	private class SampleServerHandleEditor : Editor {
		private SampleServerHandle _instance;

		private void OnEnable() {
			_instance = (SampleServerHandle)target;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			if (GUILayout.Button("UnityPing")) {
				_instance.UnityPing();
			}
			
			if (GUILayout.Button("InitData")) {
				_instance.InitData("test");
			}
			
			if (GUILayout.Button("SetKeyboardCapture")) {
				_instance.SetKeyboardCapture((!_instance._keycapture).ToString());
			}
			
			if (GUILayout.Button("ActivateShift")) {
				_instance.ActivateShift((!_instance._inShift).ToString());
			}
			
			if (GUILayout.Button("SetCursorCapture")) {
				_instance.SetCursorCapture((!_instance._cursorActive).ToString());
			}
			
			if (GUILayout.Button("LoadSample")) {
				_instance._loading.StartLoading();
			}
		}
	}
#endif
}
