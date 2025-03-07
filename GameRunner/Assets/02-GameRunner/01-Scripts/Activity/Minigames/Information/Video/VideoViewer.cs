using System;
using Cohort.GameRunner.Minigames;
using Cohort.Networking.Spaces;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// This class is used to display a video as a minigame. The game continiues when the video has finished playing.
/// </summary>
public class VideoViewer : Minigame {
    protected override float CorrectVisualDuration {
        get { return 0f; }
    }
    protected override float FaultiveVisualDuration {
        get { return 0f; }
    }
    protected override float FinishedVisualDuration {
        get { return 0f; }
    }
    public override int Score { get; set; }
    
    private const float VOLUME_SCALAR = 1f;

    public bool Interactable {
        get { return _interactable; }
        set {
            _interactable = value;
            _playPause.interactable = value;
            _stop.interactable = value;
            _audioTgl.interactable = value;
            _volume.interactable = value;
        }
    }

    private bool _interactable;

    [SerializeField] private VideoPlayer _player;
    [SerializeField] private RawImage _viewport;
    
    [SerializeField] private Toggle _playPause;
    [SerializeField] private Button _stop;
    
    [SerializeField] private Toggle _audioTgl;
    [SerializeField] private Slider _volume;
    
    private VideoInfo _data;
    private RenderTexture _tex;
    
    private bool _initialized;
    private float _vol = 1;
    private bool _audioEnabled;
    private bool _stopped = true;
    
    protected override void Awake() {
        _player.isLooping = false;
        
        _playPause.onValueChanged.AddListener(PlayPause);
        _stop.onClick.AddListener(Stop);
        _audioTgl.onValueChanged.AddListener(SetAudioEnabled);
        _volume.onValueChanged.AddListener(SetVolume);
        
        Interactable = false;
        _player.prepareCompleted += OnPrep;
        _player.loopPointReached += OnVideoFinished;
        
        base.Awake();
    }

    private void OnDestroy() {
        _playPause.onValueChanged.RemoveListener(PlayPause);
        _stop.onClick.RemoveListener(Stop);
        _audioTgl.onValueChanged.RemoveListener(SetAudioEnabled);
        _volume.onValueChanged.RemoveListener(SetVolume);
        
        _player.prepareCompleted -= OnPrep;
    }

    public override void Initialize(string gameData, float timeLimit, int minScore, int maxScore,
                                    Action<FinishCause, int> onFinished, Action onExit) {
        base.Initialize(gameData, timeLimit, minScore, maxScore, onFinished, onExit);
        
        _data = JsonUtility.FromJson<VideoInfo>(gameData);
        
        _player.url = AssetRequest.GetDownloadURL(_data.uuid);
        _player.Prepare();
    }

    public override void FinishMinigame() {
        base.FinishMinigame(FinishCause.FinPointless);
    }

    private void OnPrep(VideoPlayer source) {
        _tex = new CustomRenderTexture((int) _player.width, (int) _player.height, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
        
        //set video player output to texture
        _player.targetTexture = _tex;
        _player.StepForward();
        
        //assign texture to renderer
        _viewport.texture = _tex;
        
        _initialized = true;
        Interactable = true;

        if (_playPause.isOn) {
            _player.Play();
            
            _viewport.color = Color.white;
            _stopped = false;
        }
        else {
            _viewport.color = Color.black;
            _stopped = true;
        }
    }
    
    private void OnVideoFinished(VideoPlayer source) {
        FinishMinigame();
    }

    private void PlayPause(bool isPlaying) {
        if (!_initialized)
            return;
        
        if (_stopped) {
            _viewport.color = Color.white;
            _stopped = false;
        }
        
        if (isPlaying) {
            _player.Play();
        }
        else {
            _player.Pause();
        }
    }

    private void Stop() {
        _playPause.isOn = false;
        _stopped = true;
        
        _player.time = 0f;
        _viewport.color = Color.black;
    }

    private void SetAudioEnabled(bool isEnabled) {
        _audioEnabled = isEnabled;
        
        if (!isEnabled) {
            _volume.SetValueWithoutNotify(0f);
        }
        else {
            _volume.SetValueWithoutNotify(_vol);
        }
        
        _player.SetDirectAudioVolume(0, (_audioEnabled)?_vol * VOLUME_SCALAR : 0f);
    }

    private void SetVolume(float volume) {
        _vol = volume;
        _player.SetDirectAudioVolume(0, volume * VOLUME_SCALAR);

        _audioTgl.isOn = _vol > 0.01f;
    }
}
