using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cohort.GameRunner.Minigames;
using Cohort.Networking.Spaces;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class InteractiveVideo : VideoViewer
{

    
    public InteractiveButton _interactiveButton;
    public InteractiveVideoFeedback interactiveVideoFeedback;
    public double videoTime => _player.time;
    public InteractiveVideoData InteractiveVideoData;
    private List<Popup> _popups;
    public Transform buttonParent;
    private float WIND_UP_TIME = 1f;
    private List<float> _answers = new List<float>();
    private bool videoFinished;
    private int popupIndex;


    
    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore,
        Action<FinishCause, int> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);

        BuildGame();
    }
    
    private void BuildGame()
    {
        _popups = new List<Popup>();
        _popups.AddRange(InteractiveVideoData.popups);
        _popups =_popups.OrderBy(n => n).ToList();
        _answers.Clear();
    }

    protected override void OnVideoFinished(VideoPlayer source)
    {
        Score = _scoreRange.GetValueRound(_answers.Average(), true);
        Debug.LogError(Score);
        base.OnVideoFinished(source);
    }

    protected void Update()
    {
        if (_player.isPlaying && _player.time <  _player.length && _player.time > WIND_UP_TIME)
        {
            if (popupIndex < _popups.Count && _player.time >= _popups[popupIndex].timestamp - WIND_UP_TIME)
            {
                var button = Instantiate(_interactiveButton, buttonParent);
                button.button.onClick.AddListener(() => OnClick(button));
                button.Initialise(_popups.First());
                button.StartAnimation();
                PlaceChildRandomly((RectTransform) buttonParent, (RectTransform) button.transform);
                popupIndex++;
            }
        }
    }
    
    private void OnClick(InteractiveButton button)
    {
        ((RectTransform) interactiveVideoFeedback.transform).anchoredPosition =
            ((RectTransform) button.transform).anchoredPosition;
        int percent = (int) (button.GetClickAccuracy() * 100f);
        interactiveVideoFeedback.PlayFeedback($"{percent.ToString()}%");
        if (button.dummy)
        {
            _answers.Add(0f);
        }
        else
        {
            _answers.Add(button.GetClickAccuracy());
        }

        Destroy(button.gameObject);
    }


    void PlaceChildRandomly(RectTransform parentTransform,RectTransform childTransform)
    {
        if (parentTransform == null || childTransform == null)
        {
            Debug.LogError("Assign both parent and child RectTransforms!");
            return;
        }

        // Get parent's size
        float parentWidth = parentTransform.rect.width;
        float parentHeight = parentTransform.rect.height;

        // Get child's size
        float childWidth = childTransform.rect.width;
        float childHeight = childTransform.rect.height;

        // Calculate random anchored position, ensuring child remains within bounds
        float randomX = UnityEngine.Random.Range(-parentWidth / 2 + childWidth / 2, parentWidth / 2 - childWidth / 2);
        float randomY =  UnityEngine.Random.Range(-parentHeight / 2 + childHeight / 2, parentHeight / 2 - childHeight / 2);

        // Set the local position
        childTransform.anchoredPosition = new Vector2(randomX, randomY);
    }

}

[CustomEditor(typeof(InteractiveVideo))]
public class CreatePopup : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InteractiveVideo script = (InteractiveVideo)target;
        SerializedObject so = new SerializedObject(target);

        if (GUILayout.Button("Create popup"))
        {
            EditorGUIUtility.systemCopyBuffer = script.videoTime.ToString();
            Popup popup = CreateInstance<Popup>();
            popup.name = "Popup";
            popup.timestamp = (float) script.videoTime;
            
            
            string folderPath = "Assets/02-GameRunner/02-Assets/Minigames/InteractiveVideo/Scriptableobjects/Popups"; // Change this to your target folder
            string searchName = "Popup"; // Change this to your desired name

            // Get all asset GUIDs in the specified folder
            string[] assetGUIDs = AssetDatabase.FindAssets("", new[] { folderPath });

            // Count assets that match the given name
            int count = assetGUIDs
                .Select(AssetDatabase.GUIDToAssetPath)
                .Count(path => Path.GetFileNameWithoutExtension(path).Contains(searchName));

            Debug.Log($"Found {count} assets named '{searchName}' in {folderPath}");

            AssetDatabase.CreateAsset(popup, $"{folderPath}/{searchName}_{count}.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(script);
            so.ApplyModifiedProperties();
            Debug.Log($"Copied: {script.videoTime}");
        }
    }
}
