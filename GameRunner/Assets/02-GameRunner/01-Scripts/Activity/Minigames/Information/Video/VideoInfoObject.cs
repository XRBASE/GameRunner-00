#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Cohort/Information/Video", fileName = "Info_VI")]
public class VideoInfoObject : ScriptableObject
{
	[SerializeField] private VideoInfo _data;
	
	[CustomEditor(typeof(VideoInfoObject))]
	private class TextInfoObjectEditor : Editor {
		private VideoInfoObject _instance;
		private string _dataInput;

		private void OnEnable() {
			_instance = (VideoInfoObject)target;
		}

		public override void OnInspectorGUI() {
			DrawDefaultInspector();
			_dataInput = EditorGUILayout.TextField("json input:", _dataInput);

			if (GUILayout.Button("To json")) {
				_dataInput = JsonUtility.ToJson(_instance._data);
				GUIUtility.systemCopyBuffer = JsonUtility.ToJson(_instance._data);
				Debug.Log("Copied data to clipboard!");
			}

			if (GUILayout.Button("From json")) {
				_instance._data = JsonUtility.FromJson<VideoInfo>(_dataInput);
			}
		}
	}
}
#endif