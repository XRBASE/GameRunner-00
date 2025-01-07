using System;
using Cohort.GameRunner.InformationPoints;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextViewer : InfoViewer {
    [SerializeField] private GameObject _title;
    [SerializeField] private TMP_Text _titleField;
    [SerializeField] private TMP_Text _bodyField;
    
    [SerializeField] private Button _exit;
    [SerializeField] private Button _next;
    [SerializeField] private Button _prev;
    

    private int _pageId;
    private TextInfo _data;
    private Action _onInfoClosed;
    
    public override void Initialize(string infoData, Action onInfoClosed) {
        _data = JsonUtility.FromJson<TextInfo>(infoData);
        _onInfoClosed = onInfoClosed;

        _pageId = 0;

        _next.onClick.AddListener(NextPage);
        _prev.onClick.AddListener(PrevPage);
        _exit.onClick.AddListener(CloseInfoViewer);
        
        ShowPage();
    }

    private void OnDestroy() {
        _next.onClick.RemoveListener(NextPage);
        _prev.onClick.RemoveListener(PrevPage);
        _exit.onClick.RemoveListener(CloseInfoViewer);
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

    public override void CloseInfoViewer() {
        _onInfoClosed?.Invoke();
    }
}
