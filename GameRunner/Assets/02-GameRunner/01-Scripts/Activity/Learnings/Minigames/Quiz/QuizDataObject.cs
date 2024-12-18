using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Cohort/Learnings/QuizData", fileName = "QuizData")]
public class QuizDataObject : ScriptableObject {
	[SerializeField] private QuizData _data;

	[CustomEditor(typeof(QuizDataObject))]
	protected class QuizObjectEditor : Editor {
		private QuizDataObject _instance;
		private string _dataInput;

		protected virtual void OnEnable() {
			_instance = (QuizDataObject)target;
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
				_instance._data = JsonUtility.FromJson<QuizData>(_dataInput);
			}
		}
	}
}
#endif