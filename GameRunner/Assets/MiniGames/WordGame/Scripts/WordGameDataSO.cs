using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WordGameData", menuName = "Cohort/WordGameData", order = 0)]
public class WordGameDataSO : ScriptableObject
{
    public WordGameData wordGameData;

    public string wordGameDataJson()
    {
        return JsonUtility.ToJson(wordGameData);
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(WordGameDataSO))]
    private class WordGameDataSoEditor : Editor
    {
        private string folderName = "/WordGame";
        private string fileName = "/WordGameDataJson.txt";
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var instance = (WordGameDataSO) target;
            EditorGUILayout.HelpBox("Serialise wordDictionary from Json", MessageType.Info);

            if (GUILayout.Button("Write"))
            {
                string folderPath = Application.streamingAssetsPath + folderName;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = folderPath + fileName;
                StreamWriter writer = new StreamWriter(filePath, false);
                writer.WriteLine(instance.wordGameDataJson());
                writer.Close();
            }
            
            if (GUILayout.Button("Read"))
            {
                string folderPath = Application.streamingAssetsPath + folderName;
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                string filePath = folderPath + fileName;
                StreamReader reader = new StreamReader(filePath);
                SerializedObject so = new SerializedObject(instance);
                instance.wordGameData = JsonUtility.FromJson<WordGameData>(reader.ReadToEnd());

                reader.Close();
                filePath = folderPath + "/WordListDutch.csv";
                List<List<string>> csvData = new List<List<string>>();
                using (reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null) 
                    {
                        string[] values = line.Split(',');


                        csvData.Add(new List<string>(values));
                    }
                }
                EditorUtility.SetDirty(instance);
                so.ApplyModifiedProperties();
            }
            
            if (GUILayout.Button("Copy WordGameDataJson to clipboard"))
            {
                GUIUtility.systemCopyBuffer = instance.wordGameDataJson();
            }
        }
    }
#endif
}
