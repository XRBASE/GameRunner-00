using TMPro;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ArrangeSlot : UIPointerOver
{
    public enum ArrangeType{Submission, Deposit}
    public bool occupied;
    public ArrangeElement occupiedArrangeElement;
    public Image image;
    public TextMeshProUGUI description;
    public string correctId;
    public PlayableDirector playableDirector;
    public PlayableAsset correctFeedback, incorrectFeedback;

    public void Initialise(string correctId, string descriptionText)
    {
        this.correctId = correctId;
        description.text = descriptionText;
    }

    public void SetOccupiedArrangeElement(ArrangeElement arrangeElement)
    {
        arrangeElement.transform.SetParent(transform);
        occupiedArrangeElement = arrangeElement;
        image.sprite = arrangeElement.sprite;
        SetImageVisible(true);
        occupied = true;
    }

    public void ClearSlot()
    {
        image.sprite = null;
        occupiedArrangeElement = null;
        SetImageVisible(false);
        occupied = false;
    }

    public void SetImageVisible(bool visible)
    {
        image.gameObject.SetActive(visible);
    }

    public void Reset()
    {
        SetImageVisible(occupied);
    }

    public void PlayCorrectFeedback()
    {
        StartPlayable(correctFeedback);
    }
    
    public void PlayInCorrectFeedback()
    {
        StartPlayable(incorrectFeedback);
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
