using UnityEngine;

using Cohort.GameRunner.AvatarAnimations;
using Cohort.GameRunner.Audio;
using Cohort.Patterns;

public class PlayerAudioManager : Singleton<PlayerAudioManager> 
{
    [SerializeField] private PlayerAudioController _controllerPrefab;
    [SerializeField] private CharacterAudioSet _defaultSet;

    public PlayerAudioController SetupPlayerAudio(GameObject audioObject, CharAnimator animator, CharacterAudioSet set = null) {
        PlayerAudioController controller = audioObject.GetComponentInChildren<PlayerAudioController>();
        
        if (controller == null) {
            controller = Instantiate(_controllerPrefab, audioObject.transform);
        }
        controller.Initialize(animator, set != null ? set : _defaultSet);

        return controller;
    }
}
