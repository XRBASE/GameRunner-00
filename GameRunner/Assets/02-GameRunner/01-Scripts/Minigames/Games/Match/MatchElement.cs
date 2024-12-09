using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchElement : MonoBehaviour
{
    public int id;
    public MatchType matchType;
    public TextMeshProUGUI title;
    public Button button;
    public Image highLight;
    private MatchPreview _matchPreview;
    public Action<MatchElement> onMatchSelected;
    private bool _selectState;
    private Color _originalColor;
    public Color highlightColor;
    
    public enum MatchType{Image, Text}

    protected virtual void Awake()
    {
        _originalColor = highLight.color;
        button.onClick.AddListener(SelectMatch);
    }

    public void SetPreviewElement(MatchPreview matchPreview)
    {
        switch (matchPreview)
        {
            case ImagePreview:
                matchType = MatchType.Image;
                break;
            case TextPreview:
                matchType = MatchType.Text;
                break;
        }
        _matchPreview = matchPreview;
    }

    private void SelectMatch()
    {
        onMatchSelected?.Invoke(this);
    }

    public void SetSelectState(bool active)
    {
        _selectState = active;
        if (active)
        {
            highLight.color = highlightColor;
        }
        else
        {
            highLight.color = _originalColor;
        }
        
    }



}
