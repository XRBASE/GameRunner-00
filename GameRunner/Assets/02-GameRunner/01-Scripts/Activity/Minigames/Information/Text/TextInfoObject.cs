#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "Cohort/Information/TextInfo", fileName = "Info_TX")]
public class TextInfoObject : ScriptableObject {
	[SerializeField] private TextInfo _data;
	
	[CustomEditor(typeof(TextInfoObject))]
	private class TextInfoObjectEditor : Editor {
		private TextInfoObject _instance;
		private string _dataInput;

		private void OnEnable() {
			_instance = (TextInfoObject)target;
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
				_instance._data = JsonUtility.FromJson<TextInfo>(_dataInput);
			}
		}
	}
}
#endif