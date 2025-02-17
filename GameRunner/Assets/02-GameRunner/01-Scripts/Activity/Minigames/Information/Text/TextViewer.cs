using System;
using Cohort.GameRunner.Minigames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the user a piece of text. It can be stylized in it's own scene and pages and titles can be added to further
/// clarify the information.
///
/// Could be further improved by adding the make file formatting rules.
/// </summary>
public class TextViewer : Minigame {
    protected override float CorrectVisualDuration {
        get { return 0f; }
    }
    protected override float FaultiveVisualDuration {
        get { return 0f; }
    }
    protected override float FinishedVisualDuration {
        get { return 0f; }
    }
    public override int Score { get; set; }
    
    [SerializeField] private GameObject _title;
    [SerializeField] private TMP_Text _titleField;
    [SerializeField] private TMP_Text _bodyField;
    
    [SerializeField] private Button _exit;
    [SerializeField] private Button _next;
    [SerializeField] private Button _prev;
    
    private int _pageId;
    private TextInfo _data;

    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore,
                                    Action<FinishCause, int> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);
        
        _data = JsonUtility.FromJson<TextInfo>(gameData);
        
        _pageId = 0;
        _next.onClick.AddListener(NextPage);
        _prev.onClick.AddListener(PrevPage);
        _exit.onClick.AddListener(FinishMinigame);
        ShowPage();
    }

    private void OnDestroy() {
        _next.onClick.RemoveListener(NextPage);
        _prev.onClick.RemoveListener(PrevPage);
        _exit.onClick.RemoveListener(FinishMinigame);
    }

    private void NextPage() {
        _pageId++;
        ShowPage();
    }

    private void PrevPage() {
        _pageId--;
        ShowPage();
    }

    private void ShowPage() {
        _titleField.text = _data.pages[_pageId].title;
        _title.gameObject.SetActive(!string.IsNullOrEmpty(_data.pages[_pageId].title));

        _bodyField.text = _data.pages[_pageId].body;
        
        _exit.gameObject.SetActive(_pageId == _data.pages.Length - 1);
        _next.gameObject.SetActive(_pageId != _data.pages.Length - 1);
        _prev.gameObject.SetActive(_pageId != 0);
    }

    public override void FinishMinigame() {
        base.FinishMinigame(FinishCause.FinPointless);
    }
}
