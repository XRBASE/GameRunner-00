using System;
using Cohort.GameRunner.Input;
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

    public enum Category
    {
        Question,
        Answer
    };

    public Category category;

    public MatchPairData.MatchType matchType
    {
        get { return _matchType; }
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
    private bool _inClick;

    public  MatchState matchState;
    public int id;
    public TextMeshProUGUI label;
    public TextMeshProUGUI matchText;
    public Image matchImage;
    public Image highLight;
    public Action<MatchElement> onSelectMatch;
    public Action onDropMatch;
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
        InputManager.Instance.LearningCursor.leftDown += LeftDown;
        InputManager.Instance.LearningCursor.leftUp += LeftUp;
    }

    private void OnDestroy()
    {
        InputManager.Instance.LearningCursor.leftDown -= LeftDown;
        InputManager.Instance.LearningCursor.leftUp -= LeftUp;
    }

    private void LeftDown()
    {
        if (matchState == MatchState.Completed)
            return;
        if (PointerOver() && !_inClick)
        {
            onSelectMatch?.Invoke(this);
            _inClick = true;
        }
    }

    private void LeftUp()
    {
        if (_inClick)
        {
            onDropMatch?.Invoke();
            _inClick = false;
        }
    }

    public bool PointerOver()
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform) transform, InputManager.Instance.LearningCursor.ScreenPosition,
            Camera.main, out localPos);
        return ((RectTransform) transform).rect.Contains(localPos);
    }

    public void Initialise(int pId, MatchPairData.MatchType pMatchType, Category pCategory, Sprite sprite, string text,
        string labelText)
    {
        id = pId;
        matchType = pMatchType;
        category = pCategory;
        matchImage.sprite = sprite;
        matchText.text = text;
        label.text = labelText;
    }


    private void IncorrectFeedback()
    {
        StartPlayable(wobblePlayable);
    }

    public void Flip()
    {
        StartPlayable(rotatePlayable);
    }

    public void Select()
    {
        SetState(MatchState.Selected);
    }

    public void Deselect()
    {
        if (matchState == MatchState.Completed)
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
        matchState = state;
        switch (state)
        {
            case MatchState.Unselected:
                highLight.color = _originalColor;
                break;
            case MatchState.Selected:
                StartPlayable(enlargePlayable);
                highLight.color = highlightColor;
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