using System;
using Cohort.GameRunner.Audio;
using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : UIPanel
{
    [SerializeField] private AudioSliderUI _masterSlider;
    [SerializeField] private AudioSliderUI _musicSlider;
    [SerializeField] private AudioSliderUI _sfXSlider;
    [SerializeField] private Button _exitButton;


    private void Awake()
    {
        _masterSlider.slider.onValueChanged.AddListener(OnMasterValueChanged);
        _musicSlider.slider.onValueChanged.AddListener(OnMusicValueChanged);
        _sfXSlider.slider.onValueChanged.AddListener(OnSFXValueChanged);
        _masterSlider.sliderTitle.text = AudioManager.Channel.Master.ToString();
        _musicSlider.sliderTitle.text = AudioManager.Channel.Music.ToString();
        _sfXSlider.sliderTitle.text = AudioManager.Channel.Sfx.ToString();
        _exitButton.onClick.AddListener(Deactivate);
    }

    public void Initialise(Action onExit)
    {
        _exitButton.onClick.AddListener(onExit.Invoke);
        Deactivate();
    }

    private void OnMasterValueChanged(float value)
    {
        AudioManager.Instance.SetAudioVolume(value, AudioManager.Channel.Master);
        _masterSlider.SetSlider(value);
    }

    private void OnMusicValueChanged(float value)
    {
        AudioManager.Instance.SetAudioVolume(value, AudioManager.Channel.Music);
        _musicSlider.SetSlider(value);
    }

    private void OnSFXValueChanged(float value)
    {
        AudioManager.Instance.SetAudioVolume(value, AudioManager.Channel.Sfx);
        _sfXSlider.SetSlider(value);
    }

    private void OnEnable()
    {
        _masterSlider.SetSlider(AudioManager.Instance.GetAudioVolume(AudioManager.Channel.Master));
        _musicSlider.SetSlider(AudioManager.Instance.GetAudioVolume(AudioManager.Channel.Music));
        _sfXSlider.SetSlider(AudioManager.Instance.GetAudioVolume(AudioManager.Channel.Sfx));
    }
}