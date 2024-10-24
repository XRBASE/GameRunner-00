using System;
using Cohort.Patterns;
using TMPro;
using UnityEngine;

public class ScoreboardEntry : MonoBehaviour, ObjectPool<HighscoreTracker.PlayerScore, ScoreboardEntry>.IPoolable
{
    public bool IsActive {
        get { return gameObject.activeSelf;}
        set { gameObject.SetActive(value);}
    }

    [SerializeField] private TMP_Text nameField;
    [SerializeField] private TMP_Text scoreField;
    
    public void UpdatePoolable(int index, HighscoreTracker.PlayerScore data) {
        nameField.text = data.name;
        scoreField.text = data.score.ToString();
    }

    public ScoreboardEntry Copy() {
        return Instantiate(this, transform.parent);
    }
}
