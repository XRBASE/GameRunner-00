using System;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Input;
using Cohort.GameRunner.Minigames;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ArrangeGame : Minigame
{
    protected override float CorrectVisualDuration { get; }
    protected override float FaultiveVisualDuration { get; }
    protected override float FinishedVisualDuration { get; }
    public override float Score { get; set; }

    public ArrangeGameLibrarySO arrangeGameLibrary;
    private ArrangeGameData _arrangeGameData;
    public ArrangeSlot arrangeSlotPrefab;
    public Transform depositSlots;
    public Transform submissionSlots;
    public FollowCursor followCursor;
    public Image copyImage;
    private ArrangeElement _selectedElement;
    private ArrangeSlot _originHoverSlot;
    private List<ArrangeSlot> _slots = new List<ArrangeSlot>();
    private List<ArrangeSlot> _depositSlots = new List<ArrangeSlot>();
    private List<ArrangeSlot> _submissionSlots = new List<ArrangeSlot>();
    private static Random _rng = new Random();
    
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


    public override void Initialize(string gameData, float timeLimit, Action<FinishCause, float> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, onFinished, onExit);
        _arrangeGameData = JsonUtility.FromJson<ArrangeGameData>(gameData);
        BuildGame();

        
    }

    private void BuildGame()
    {
        _depositSlots.Clear();
        _submissionSlots.Clear();
        _slots.Clear();
        foreach (var id in _arrangeGameData.chosenIds)
        {
            ArrangeData arrangeData = arrangeGameLibrary.arrangeGameData.First(data => data.UID == id);
            var depositSlot = Instantiate(arrangeSlotPrefab, depositSlots);
            depositSlot.Initialise(ArrangeSlot.ArrangeType.Deposit,"", "");

            var submissionSlot = Instantiate(arrangeSlotPrefab, submissionSlots);
            submissionSlot.Initialise(ArrangeSlot.ArrangeType.Submission, id, arrangeData.text);

            var arrangeElement = new GameObject("ArrangeElement").AddComponent<ArrangeElement>();
            arrangeElement.Initialise(arrangeData);
            
            depositSlot.SetOccupiedArrangeElement(arrangeElement);
            
            _depositSlots.Add(depositSlot);
            _submissionSlots.Add(submissionSlot);
        }
        
        _slots.AddRange(_depositSlots);
        _slots.AddRange(_submissionSlots);
        ShuffleDepositSlots();
    }
    
    private void ShuffleDepositSlots()
    {
        List<int> places = new List<int>();
        for (int i = 0; i < _depositSlots.Count; i++)
        {
            places.Add(i);
        }
        Shuffle(places);
        for (int i = 0; i < _depositSlots.Count; i++)
        {
            _depositSlots[i].transform.SetSiblingIndex(places[i]);
        }
        
    }

    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
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
        _originHoverSlot?.Reset();
        followCursor.gameObject.SetActive(false);
    }

    private bool HoverOverSlot(out ArrangeSlot hoverSlot)
    {
        hoverSlot = _slots.FirstOrDefault(slot => slot.PointerOver());
        return hoverSlot != null;
    }

    public void Submit()
    {
        if (_submissionSlots.Any(slot => !slot.occupied))
        {
            //not all submission slots are filled in
            Debug.LogError("not all submission slots are filled in");
            return;
        }
        
        int correctSubmissions =0;
        for (int i = 0; i < _submissionSlots.Count; i++)
        {
            if (_submissionSlots[i].correctId == _submissionSlots[i].occupiedArrangeElement.id)
            {
                //correct
                correctSubmissions++;
                Debug.LogError("Correct");
            }
            else
            {
                //incorrect
                Debug.LogError("Incorrect");
            }
        }

        float correctPercent = (float)correctSubmissions/_submissionSlots.Count;
        Debug.LogError(correctPercent);

    }
}