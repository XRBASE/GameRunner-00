using System;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public Action<InteractiveButton> onTimeout;
    private const float APEX_TIME = .6f;


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
            onTimeout?.Invoke(this);
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
        if (dummy)
            return 0f;
        float apex = GetClickApex();
        float apexDeviation = _timer - apex;
        apexDeviation = Mathf.Abs(apexDeviation);
        float accuracy = ((apex - apexDeviation) / apex);
        if (apexDeviation < APEX_TIME/2)
        {
            return 1f;
        }
        if(accuracy <.1f)
        {
            return 0f;
        }
        return accuracy;
    }

    public float GetClickApex()
    {
        return (float) _playableDirector.duration / 2;
    }
    
    
}
