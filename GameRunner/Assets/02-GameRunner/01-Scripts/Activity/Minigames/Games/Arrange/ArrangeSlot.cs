using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Before ArrangeElement
[DefaultExecutionOrder(100)]
public class ArrangeSlot : UIPointerOver
{
    public enum ArrangeType{Submission, Deposit}
    public ArrangeType arrangeType;
    public bool occupied;
    public ArrangeElement occupiedArrangeElement;
    public Image image;
    public TextMeshProUGUI description;
    public int correctId;

    private void Awake()
    {
        if(occupiedArrangeElement!= null)
            SetOccupiedArrangeElement(occupiedArrangeElement);
        switch (arrangeType)
        {
            case ArrangeType.Deposit:
                description.gameObject.SetActive(false);
                break;
            case ArrangeType.Submission:
                description.gameObject.SetActive(true);
                break;
        }
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
    
}
