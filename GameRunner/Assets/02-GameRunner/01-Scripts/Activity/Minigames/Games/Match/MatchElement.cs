using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class MatchElement : MonoBehaviour
{
    public enum MatchState
    {
        Unselected,
        Selected,
        Completed,
        Wrong
    }

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
    private MatchState _matchState;

    public int id;
    public TextMeshProUGUI title;
    public TextMeshProUGUI matchText;
    public Image matchImage;
    public Button button;
    public Image highLight;
    public Action<MatchElement> onMatchSelected;
    private Color _originalColor;
    public Color highlightColor;
    public Color completedColor;
    public Color wrongColor;
	public PlayableDirector playableDirector;
	public PlayableAsset enlargePlayable;
    public PlayableAsset wobblePlayable;
    public PlayableAsset rotatePlayable;


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

    private void IncorrectFeedback()
    {
        StartPlayable(wobblePlayable);
    }

    public void Flip()
    {
        StartPlayable(rotatePlayable);
    }

    private void SelectMatch()
    {
        if (_matchState == MatchState.Completed)
            return;
        onMatchSelected?.Invoke(this);
    }

    public void Select()
    {
        SetState(MatchState.Selected);
    }
    
    public void Deselect()
    {
        if(_matchState == MatchState.Completed)
            return;
        SetState(MatchState.Unselected);
    }

    public void Complete()
    {
        SetState(MatchState.Completed);
    }

    public void WrongAnswer()
    {
        SetState(MatchState.Wrong);
    }

    private void SetState(MatchState state)
    {
        _matchState = state;
        switch (state)
        {
            case MatchState.Unselected:
                highLight.color = _originalColor;
                break;
            case MatchState.Selected:
                StartPlayable(enlargePlayable);
                highLight.color  = highlightColor;
                break;
            case MatchState.Wrong:
                IncorrectFeedback();
                highLight.color = wrongColor;
                break;
            case MatchState.Completed:
                StartPlayable(enlargePlayable);
                highLight.color = completedColor;
                break;
            
        }
    }
    
    private void StartPlayable(PlayableAsset playable)
    {
        playableDirector.time = 0;
        playableDirector.Stop();
        playableDirector.Evaluate();
        playableDirector.playableAsset = playable;
        playableDirector.Play();
    }
}