using UnityEngine;

using Cohort.GameRunner.AvatarAnimations;
using Cohort.GameRunner.Audio;

[DefaultExecutionOrder(0)] //Before CharAnimator
public class PlayerAudioController : MonoBehaviour {
    [SerializeField] private CharacterAudioSet characterAudioSet;
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioSource _oneshotSource;

    private float _flux;
    private CharAnimator _animator;
    private CharClipEntry _curClip;
    
    public void Initialize(CharAnimator animator, CharacterAudioSet characterAudioOverride) {
        characterAudioSet = characterAudioOverride;
        _animator = animator;
        _animator.onStateChange += OnAnimationStateChanged;
        enabled = false;
    }

    private void OnDestroy() {
        if (_animator != null) {
            _animator.onStateChange -= OnAnimationStateChanged;
        }
    }

    private void OnAnimationStateChanged(CharAnimator.AnimationState state) {
        if (characterAudioSet.TryGetClip(state, out CharClipEntry clip)) {
            if (_source.isPlaying && !_curClip.oneShot) {
                _source.Stop();
            }
            
            _source.loop = clip.loop;
            _source.clip = clip.audio;
            _curClip = clip;
            if (clip.oneShot && !clip.loop) {
                _oneshotSource.pitch = GetPitch();
                _oneshotSource.PlayOneShot(clip.audio);
                return;
            }
            else {
                _source.Play();
            }
            
            enabled = _curClip.scaled;
            _source.pitch = GetPitch();
            return;
        }
        
        if (_curClip != null && state != _curClip.state && _curClip.loop) {
            _source.Stop();
            enabled = false;
            _curClip = null;
            
            return;
        }
        
        enabled = false;
    }

    private float GetPitch() {
        float pitch = 1;
        
        if (_curClip.scaled) {
            if (Mathf.Abs(_curClip.pitchflux) >= 0.0001f) {
                _flux = Random.Range(-_curClip.pitchflux, _curClip.pitchflux);
            }
            else {
                _flux = 0f;
            }
        
            pitch = _curClip.basePitch + _flux;
            if (!string.IsNullOrEmpty(_curClip.scaleParamName)) {
                pitch += _animator.GetFloat(_curClip.scaleParamName);
            }
        }

        return pitch;
    }

    private void Update() {
        _source.pitch = GetPitch();
    }
}
