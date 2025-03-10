using System;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class InteractiveButton : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _playableDirector;
    [SerializeField]
    private TextMeshProUGUI _textMeshProUGUI;
    public Button button;
    private float _timer = 0f;
    public bool dummy;


    public void Initialise(Popup popup)
    {
        SetText(popup.text);
        dummy = popup.dummy;
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > _playableDirector.duration)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        _textMeshProUGUI.text = text;
    }
    
    public void StartAnimation()
    {
        _playableDirector.Play();
    }

    public float GetClickAccuracy()
    {
        float apex = (float)_playableDirector.duration / 2;
        float apexDeviation = (float) _playableDirector.time - apex;
        apexDeviation = Mathf.Abs(apexDeviation);
        if (apexDeviation < .2f)
        {
            return 1f;
        }
        else
        {
            return ((apex - apexDeviation) / apex);
        }
    }
    
    
}
