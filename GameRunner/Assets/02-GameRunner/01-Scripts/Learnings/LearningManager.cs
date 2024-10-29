using Cohort.Patterns;
using UnityEngine;

public class LearningManager : Singleton<LearningManager> {
    public const float MIN_TIME = 2;
    public const float MAX_TIME = 5;
    
    public LearningDescription this[int index] {
        get { return _settings.learnings[index];}
    }
    
    [SerializeField] private LearningCycleDescription _settings;
    [SerializeField] private NetworkedTimer _timer;
    
    private int _scoreMultiplier;

    public void OnActivityStart(int scoreMultiplier) {
        _scoreMultiplier = scoreMultiplier;

        _timer.onFinish.AddListener(OnTimerFinished);
    }

    public void OnActivityStop() {
        _timer.onFinish.RemoveListener(OnTimerFinished);
    }
    
    private void StartTimer() {
        float duration = Random.Range(MIN_TIME, MAX_TIME);
        _timer.StartTimer(duration);
    }

    private void OnTimerFinished() {
        //start minigame
        
        StartTimer();
    } 

    

    public void InitializeLearning(Learning learning) {
        
    }
}
