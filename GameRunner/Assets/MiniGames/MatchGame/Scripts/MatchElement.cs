using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchElement : MonoBehaviour
{
    private MatchPairData.MatchType matchType
    {
        get { return _matchType;}
        set
        {
            _matchType = value;
            switch (value)
            {
                case MatchPairData.MatchType.Image:
                    matchText.gameObject.SetActive(false);
                    matchImage.gameObject.SetActive(true);
                    break;
                case MatchPairData.MatchType.Text:
                    matchText.gameObject.SetActive(true);
                    matchImage.gameObject.SetActive(false);
                    break;
            }
        }
    }

    private MatchPairData.MatchType _matchType;

    public int id;
    public TextMeshProUGUI title;
    public TextMeshProUGUI matchText;
    public Image matchImage;
    public Button button;
    public Image highLight;
    public Action<MatchElement> onMatchSelected;
    private Color _originalColor;
    public Color highlightColor;


    protected virtual void Awake()
    {
        _originalColor = highLight.color;
        button.onClick.AddListener(SelectMatch);
    }

    public void SetPreviewElement(Sprite sprite)
    {
        matchType = MatchPairData.MatchType.Image;
        matchImage.sprite = sprite;
    }

    public void SetPreviewElement(string text)
    {
        matchType = MatchPairData.MatchType.Text;
        matchText.text = text;
    }

    private void SelectMatch()
    {
        onMatchSelected?.Invoke(this);
    }

    public void SetSelectState(bool active)
    {
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