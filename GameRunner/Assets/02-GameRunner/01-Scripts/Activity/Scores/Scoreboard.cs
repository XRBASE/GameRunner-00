using Cohort.Patterns;
using System.Linq;
using UnityEngine;

public class Scoreboard : MonoBehaviour {
    [SerializeField] private ScoreboardEntry _templateEntry;
    [SerializeField] private int _maxEntries;
    
    private ObjectPool<HighscoreTracker.PlayerScore, ScoreboardEntry> _pool;

    private void Awake() {
        _pool = new ObjectPool<HighscoreTracker.PlayerScore, ScoreboardEntry>(_templateEntry);
        
        HighscoreTracker.Instance.onScoresUpdated += OnScoresUpdated;
        OnScoresUpdated(HighscoreTracker.Instance.GetScores());
    }

    private void OnDestroy() {
        HighscoreTracker.Instance.onScoresUpdated -= OnScoresUpdated;
    }

    private void OnScoresUpdated(HighscoreTracker.PlayerScore[] scores) {
        _pool.SetAll(scores.Skip(0).Take(_maxEntries));
    }
}
