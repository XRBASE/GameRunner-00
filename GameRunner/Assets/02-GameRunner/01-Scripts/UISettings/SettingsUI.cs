using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : UIPanel
{
    [SerializeField] private AudioSettingsUI _audioSettingsUI;
    [SerializeField] private Button _audioSettings;
    [SerializeField] private Button _exit;


    private void Awake()
    {
        _exit.onClick.AddListener(Deactivate);
        _audioSettings.onClick.AddListener(ActivateAudioSettings);
        _audioSettingsUI.Initialise(Activate);
    }

    private void ActivateAudioSettings()
    {
        _audioSettingsUI.Activate();
        Deactivate();
    }
}