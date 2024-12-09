using TMPro;
using UnityEngine;
using UnityEngine.Playables;

public class ScoreUI : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public TextMeshProUGUI scoreTMP;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void PlayScore(int score)
    {
        gameObject.SetActive(true);
        scoreTMP.text = score.ToString();
        playableDirector.Play();

    }
}
