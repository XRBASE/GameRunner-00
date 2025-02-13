using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cohort.GameRunner.Input;
using Cohort.GameRunner.Minigames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class ArrangeGame : Minigame
{
    protected override float CorrectVisualDuration { get; }
    protected override float FaultiveVisualDuration { get; }
    protected override float FinishedVisualDuration
    {
        get { return 2f; }
    }

    public override float Score { get; set; }
    
    private const float FEEDBACK_INTERVAL = .5f;

    public TextMeshProUGUI title;
    public ArrangeGameLibrarySO arrangeGameLibrary;
    private ArrangeGameData _arrangeGameData;
    public ArrangeSlot arrangeSlotPrefab;
    public Transform submissionSlots;
    public FollowCursor followCursor;
    public Image copyImage;
    public AudioSource feedbackAudio;
    public AudioSource pitchedAudio;
    public AudioClip dragAudioClip, dropAudioClip, correctAudioClip, incorrectAudioClip, completedAudioClip;
    private ArrangeElement _selectedElement;
    private ArrangeSlot _originHoverSlot;
    private List<ArrangeSlot> _slots = new List<ArrangeSlot>();
    private List<ArrangeElement> _arrangeElements = new List<ArrangeElement>();
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
    
    private void LeftDown()
    {
        if(!IsPlaying)
            return;
        //Select   
        if (HoverOverSlot(out var hoverSlot) && hoverSlot.occupied)
        {
            _originHoverSlot = hoverSlot;
            _originHoverSlot.SetImageVisible(false);
            _selectedElement = hoverSlot.occupiedArrangeElement;
            SetCopy();
            feedbackAudio.PlayOneShot(dragAudioClip);
        }
    }


    private void LeftUp()
    {
        if(!IsPlaying)
            return;
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
                feedbackAudio.PlayOneShot(dropAudioClip);
                hoverSlot.SetOccupiedArrangeElement(_selectedElement);
            }
        }

        _selectedElement = null;
        _originHoverSlot?.Reset();
        followCursor.gameObject.SetActive(false);
    }


    public override void Initialize(string gameData, float timeLimit, Action<FinishCause, float> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, onFinished, onExit);
        _arrangeGameData = JsonUtility.FromJson<ArrangeGameData>(gameData);
        BuildGame();
    }

    private void BuildGame()
    {
        _slots.Clear();
        _arrangeElements.Clear();
        title.text = _arrangeGameData.title;
        foreach (var id in _arrangeGameData.chosenIds)
        {
            ArrangeData arrangeData = arrangeGameLibrary.arrangeGameData.First(data => data.UID == id);

            var submissionSlot = Instantiate(arrangeSlotPrefab, submissionSlots);
            submissionSlot.Initialise(id, arrangeData.text);

            var arrangeElement = new GameObject("ArrangeElement").AddComponent<ArrangeElement>();
            arrangeElement.Initialise(arrangeData);
            _arrangeElements.Add(arrangeElement);

            _slots.Add(submissionSlot);
        }
        
        ShuffleArrangeElements();
        IsPlaying = true;
    }
    
    private void ShuffleArrangeElements()
    {
        List<int> places = new List<int>();
        for (int i = 0; i < _arrangeElements.Count; i++)
        {
            places.Add(i);
        }
        Shuffle(places);
        for (int i = 0; i < _arrangeElements.Count; i++)
        {
            _slots[i].SetOccupiedArrangeElement(_arrangeElements[i]);
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

    private void SetCopy()
    {
        copyImage.sprite = _selectedElement.sprite;
        followCursor.gameObject.SetActive(true);
    }


    private bool HoverOverSlot(out ArrangeSlot hoverSlot)
    {
        hoverSlot = _slots.FirstOrDefault(slot => slot.PointerOver());
        return hoverSlot != null;
    }

    public void Submit()
    {
        if(!IsPlaying)
            return;
        if (_slots.Any(slot => !slot.occupied))
        {
            //not all submission slots are filled in
            Debug.LogError("not all submission slots are filled in");
            return;
        }

        IsPlaying = false;
        DoFeedbackRoutine();
    }

    private void DoFeedbackRoutine()
    {
        var routine = FeedbackRoutine();
        StartCoroutine(routine);
    }

    private IEnumerator FeedbackRoutine()
    {
        int correctSubmissions =0;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i].correctId == _slots[i].occupiedArrangeElement.id)
            {
                //correct
                correctSubmissions++;
                _slots[i].PlayCorrectFeedback();
                pitchedAudio.PlayOneShot(correctAudioClip);
                pitchedAudio.pitch += .1f;
            }
            else
            {
                //incorrect
                _slots[i].PlayInCorrectFeedback();
                pitchedAudio.PlayOneShot(incorrectAudioClip);
                pitchedAudio.pitch -= .1f;
            }
            yield return new WaitForSeconds(FEEDBACK_INTERVAL);
        }

        pitchedAudio.pitch = 1f;

        GameFinishedFeedback(correctSubmissions);

    }
    
    
    
    private void GameFinishedFeedback(int correctSubmissions)
    {
        Score = (float)correctSubmissions/_slots.Count;

        if(Score > .2f)
            feedbackAudio.PlayOneShot(completedAudioClip);
        StartCoroutine(DoTimeout(FinishedVisualDuration, FinishMinigame));
    }

}