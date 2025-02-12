using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Input;
using Cohort.GameRunner.Minigames;
using UnityEngine;
using UnityEngine.UI;

public class ArrangeGame : Minigame
{
    protected override float CorrectVisualDuration { get; }
    protected override float FaultiveVisualDuration { get; }
    protected override float FinishedVisualDuration { get; }
    public override float Score { get; set; }

    public ArrangeElement arrangeElementPrefab;
    public ArrangeSlot arrangeSlotPrefab;
    public FollowCursor followCursor;
    public Image copyImage;
    public Button submitButton;
    private ArrangeElement _selectedElement;
    private ArrangeSlot _originHoverSlot;
    public List<ArrangeSlot> slots;
    
    protected virtual void OnDestroy()
    {
        InputManager.Instance.LearningCursor.leftUp -= LeftUp;
        InputManager.Instance.LearningCursor.leftDown -= LeftDown;
    }

    protected override void Awake()
    {
        base.Awake();
        InputManager.Instance.LearningCursor.leftUp += LeftUp;
        InputManager.Instance.LearningCursor.leftDown += LeftDown;
    }

    private void LeftDown()
    {
        //Select   
        if (HoverOverSlot(out var hoverSlot) && hoverSlot.occupied)
        {
            _originHoverSlot = hoverSlot;
            _originHoverSlot.SetImageVisible(false);
            _selectedElement = hoverSlot.occupiedArrangeElement;
            SetCopy();
        }
    }

    private void SetCopy()
    {
        copyImage.sprite = _selectedElement.sprite;
        followCursor.gameObject.SetActive(true);
    }

    private void LeftUp()
    {
        //Release
        if (HoverOverSlot(out var hoverSlot))
        {
            if (_selectedElement != null)
            {
                if (hoverSlot.occupied)
                {
                    _originHoverSlot.SetOccupiedArrangeElement(hoverSlot.occupiedArrangeElement);
                }
                else
                {
                    _originHoverSlot.ClearSlot();
                }

                hoverSlot.SetOccupiedArrangeElement(_selectedElement);
            }
        }

        _selectedElement = null;
        _originHoverSlot.Reset();
        followCursor.gameObject.SetActive(false);
    }

    private bool HoverOverSlot(out ArrangeSlot hoverSlot)
    {
        hoverSlot = slots.FirstOrDefault(slot => slot.PointerOver());
        return hoverSlot != null;
    }

    private void Submit()
    {
    }
}