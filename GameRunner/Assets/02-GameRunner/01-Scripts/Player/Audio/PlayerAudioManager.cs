using Cohort.GameRunner.AvatarAnimations;
using Cohort.Patterns;
using UnityEngine;

public class PlayerAudioManager : Singleton<PlayerAudioManager> 
{
    [SerializeField] private PlayerAudioController _controllerPrefab;
    [SerializeField] private AudioSet _defaultSet;

    public PlayerAudioController SetupPlayerAudio(GameObject audioObject, CharAnimator animator, AudioSet set = null) {
        PlayerAudioController controller = audioObject.GetComponentInChildren<PlayerAudioController>();
        
        if (controller == null) {
            controller = Instantiate(_controllerPrefab, audioObject.transform);
        }
        controller.Initialize(animator, set != null ? set : _defaultSet);

        return controller;
    }
}
