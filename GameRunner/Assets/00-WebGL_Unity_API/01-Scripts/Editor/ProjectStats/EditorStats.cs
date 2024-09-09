#if UNITY_EDITOR
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

public class CodeStatsWindow : EditorWindow {
    private bool _hasStats;
    private VisualElement _root;
    private CodeStatElement _elements;
    private CodeStats _stats;
    
    [MenuItem("Cohort/Development/EditorStats")]
    public static void OpenWindow() {
        CodeStatsWindow wnd = GetWindow<CodeStatsWindow>();
        wnd.titleContent = new GUIContent("Code statistics");
    }

    public void CreateGUI() {
        _root = rootVisualElement;

        Label label = new Label("Lot's code stats at your service!");
        _root.Add(label);

        CreateStats(_stats);

        Button btn = new Button();
        btn.name = "Get stats";
        btn.text = "Get stats";
        btn.clicked += GetStats;
        _root.Add(btn);
    }

    private void CreateStats(CodeStats stats) {
        if (_elements == null) {
            _elements = CodeStatElement.Create(_root);
        }
        
        _elements.SetData(stats);
    }

    private void GetStats() {
        _stats = ProcessFiles(Application.dataPath, new CodeStats());
        
        CreateStats(_stats);
    }

    private CodeStats ProcessFiles(string dir, CodeStats stats) {
        var files = Directory.GetFiles(dir);
        string[] lines;
        foreach (string file in files) {
            string ext = Path.GetExtension(file).ToLower();
            
            if (ext == ".cs") {
                stats.files++;
                lines = File.ReadAllLines(file);
                foreach (string line in lines) {
                    if (line.Contains(" class ")) {
                        stats.classes++;
                    }

                    stats.lines++;
                    stats.charactes += (ulong)line.Trim().Length;
                }
            }
        }

        string[] dirs = Directory.GetDirectories(dir);
        foreach (string d in dirs) {
            stats = ProcessFiles(d, stats);
        }

        return stats;
    }

    private class CodeStatElement {
        public Label data;

        public void SetData(CodeStats stats) {
            CultureInfo culture = CultureInfo.CurrentCulture;

            
            data.text = $"files\t\t {stats.files:N0},\n"+
                        $"classes\t {stats.classes:N0},\n"+
                        $"lines\t\t {stats.lines:N0},\n"+
                        $"characters\t {stats.charactes:N0}.";
        }

        public static CodeStatElement Create(VisualElement root) {
            CodeStatElement em = new CodeStatElement();
            em.data = new Label();
            em.data.name = "data field";
            em.data.text = "le data goes here";
            
            root.Add(em.data);
            return em;
        }
    }

    private struct CodeStats {
        public ulong files;
        public ulong classes;
        public ulong lines;
        public ulong charactes;
    }
}
#endif