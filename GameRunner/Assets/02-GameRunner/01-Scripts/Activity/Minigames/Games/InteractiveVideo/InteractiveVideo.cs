using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

public class InteractiveVideo : VideoViewer
{
    public TextMeshProUGUI titleText;
    public InteractiveButton _interactiveButton;
    public InteractiveVideoFeedback interactiveVideoFeedback;

    public double videoTime => _player.time;

    //This needs to be serialised in the scene in which the game is played so the assets get build in the asset library
    public InteractiveVideoLibrary interactiveVideoLibrary;
    private InteractiveVideoData _interactiveVideoData;
    private List<Popup> _popups;
    public Transform buttonParent;
    private float _clickApex;
    private List<float> _answers = new List<float>();
    private bool _videoFinished;
    private int _popupIndex;
    public AudioSource feedbackAudio;
    public AudioClip correctAudioClip, inCorrectAudioClip;
    private const string TIMEOUT_TEXT = "TOO LATE";
    private const string INCORRECT_TEXT = "INCORRECT";

    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore,
        Action<FinishCause, int> onFinished, Action onExit)
    {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);
        _interactiveVideoData = JsonUtility.FromJson<InteractiveVideoData>(gameData);
        _clickApex = _interactiveButton.GetClickApex();
        BuildGame();
    }

    private void BuildGame()
    {
        titleText.text = _interactiveVideoData.titleText;
        _popups = new List<Popup>();
        foreach (var id in _interactiveVideoData.chosenIds)
        {
            var popup = interactiveVideoLibrary.popups.FirstOrDefault(item => item.UID == id);
            if (popup != null)
                _popups.Add(popup);
            else
            {
                Debug.LogError(
                    $"No pupup found in the Interactive video library wih chosenID {id}, Add all the used popups in the interactive video library");
            }
        }

        _popups = _popups.OrderBy(n => n.timestamp).ToList();
        _answers.Clear();
    }

    protected override void OnVideoFinished(VideoPlayer source)
    {
        Score = _scoreRange.GetValueRound(_answers.Average(), true);
        base.OnVideoFinished(source);
    }

    protected void Update()
    {
        if (_player.isPlaying && _player.time < _player.length && _player.time > _clickApex)
        {
            if (_popupIndex < _popups.Count && _player.time >= _popups[_popupIndex].timestamp - _clickApex)
            {
                var button = Instantiate(_interactiveButton, buttonParent);
                button.button.onClick.AddListener(() => Submit(button));
                button.Initialise(_popups[_popupIndex]);
                button.onTimeout += TimeOut;
                button.StartAnimation();
                PlaceChildRandomly((RectTransform) buttonParent, (RectTransform) button.transform);
                _popupIndex++;
            }
        }
    }

    private void Submit(InteractiveButton button)
    {
        if (button.dummy)
        {
            HandleAnswerInCorrect(button);
        }
        else
        {
            HandleAnswerCorrect(button);
        }

        Destroy(button.gameObject);
    }


    private void HandleAnswerCorrect(InteractiveButton button)
    {
        _answers.Add(button.GetClickAccuracy());
        int percent = (int) (button.GetClickAccuracy() * 100f);
        PlayTextFeedback(button, $"{percent.ToString()}%", InteractiveVideoFeedback.FeedBackState.Correct);
        feedbackAudio.PlayOneShot(correctAudioClip);
    }

    private void HandleAnswerInCorrect(InteractiveButton button)
    {
        _answers.Add(button.GetClickAccuracy());
        PlayTextFeedback(button, INCORRECT_TEXT, InteractiveVideoFeedback.FeedBackState.Incorrect);
        feedbackAudio.PlayOneShot(inCorrectAudioClip);
    }

    private void TimeOut(InteractiveButton button)
    {
        if (!button.dummy)
        {
            _answers.Add(button.GetClickAccuracy());
            PlayTextFeedback(button, TIMEOUT_TEXT, InteractiveVideoFeedback.FeedBackState.TimeOut);
            feedbackAudio.PlayOneShot(inCorrectAudioClip);
        }

        Destroy(button.gameObject);
    }


    private void PlayTextFeedback(InteractiveButton button, string text, InteractiveVideoFeedback.FeedBackState feedBackState)
    {
        ((RectTransform) interactiveVideoFeedback.transform).anchoredPosition =
            ((RectTransform) button.transform).anchoredPosition;
        interactiveVideoFeedback.PlayFeedback(text, feedBackState);
    }

    void PlaceChildRandomly(RectTransform parentTransform, RectTransform childTransform)
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

        Vector2 newPosition = Vector2.zero;

        // Calculate random anchored position, ensuring child remains within bounds
        float randomX = UnityEngine.Random.Range(-parentWidth / 2 + childWidth / 2, parentWidth / 2 - childWidth / 2);
        float randomY =
            UnityEngine.Random.Range(-parentHeight / 2 + childHeight / 2, parentHeight / 2 - childHeight / 2);

        newPosition = new Vector2(randomX, randomY);

        // Set the local position
        childTransform.anchoredPosition = newPosition;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(InteractiveVideo))]
public class CreatePopup : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        InteractiveVideo script = (InteractiveVideo) target;
        SerializedObject so = new SerializedObject(target);

        if (GUILayout.Button("Create popup"))
        {
            EditorGUIUtility.systemCopyBuffer = script.videoTime.ToString();
            Popup popup = CreateInstance<Popup>();
            popup.name = "Popup";
            popup.timestamp = (float) script.videoTime;
            popup.AssignNewUID();
            
            string folderPath =
                "Assets/02-GameRunner/02-Assets/Minigames/InteractiveVideo/Scriptableobjects/Popups"; // Change this to your target folder
            string searchName = "Popup"; // Change this to your desired name

            // Get all asset GUIDs in the specified folder
            string[] assetGUIDs = AssetDatabase.FindAssets("", new[] {folderPath});

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
#endif