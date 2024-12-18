using Cohort.GameRunner.Input.Actions;
using Cohort.GameRunner.Input.Maps;
using Cohort.Patterns;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Cohort.GameRunner.Input
{
    /// <summary>
    /// Central entry point for subscribing to input actions
    /// </summary>
    public class InputManager : Singleton<InputManager>
    {
        /// <summary>
        /// Disposed is used to check if action maps are already destroyed. Unsubscribing is only works when this is false.
        /// </summary>
        public static bool Disposed { get; private set; }

        public bool teleportEnabled = true;

        public CursorRayCaster Raycaster {
            get { return _raycaster; }
            set { _raycaster = value; }
        }

        [SerializeField] private CursorRayCaster _raycaster;
        [SerializeField] private InputSystemUIInputModule _uiInputModule;
        
        /// <summary>
        /// Returns the input action asset or loads it if it is null
        /// </summary>
        public static InputActionAsset InputActionAsset { get { return _inputActionAsset; } }
        private static InputActionAsset _inputActionAsset;
        
        /// <summary>
        /// UI input subaction set
        /// </summary>
        public UIInput UIInput { get { return _uiInput; } }
        private UIInput _uiInput;
        
        /// <summary>
        /// Cursor specific (non-action bound) input.
        /// </summary>
        public CursorInput Cursor { get { return _cursor; } }
        private CursorInput _cursor;

        public InteractInput InteractInput { get { return _interactInput; } }
        private InteractInput _interactInput;

        public TrackInputs TrackInputs { get { return _trackInputs; } }
        private TrackInputs _trackInputs;
        
        public PlayerMoveInput PlayerMove { get { return _playerMove; } }
        private PlayerMoveInput _playerMove;
        
        public CameraInput CameraInput { get { return _cameraInput; } }
        private CameraInput _cameraInput;
        
        public DialogInput DialogInput { get { return _dialogInput; } }
        private DialogInput _dialogInput;

        public DevInput DevInput { get { return _devInput; } }
        private DevInput _devInput;

        public EmoteInput EmoteInput { get { return _emoteInput; } }
        private EmoteInput _emoteInput;
        
        public GamepadInput GamepadInput { get { return _gamepadInput; } }
        private GamepadInput _gamepadInput;

        public InputDetector InputDetector { get { return _inputDetector; } }
        private InputDetector _inputDetector;
        public TypingInput TypingInput { get { return _typingInput; } }
        private TypingInput _typingInput;

        public enum ActionMaps {UI, Cursor, Interact, Track, PlayerMove,Emote, Camera, Dialog, Typing, Dev}

        private Dictionary<ActionMaps, InputActionMap> _actionMapDictionary = new Dictionary<ActionMaps, InputActionMap>();
        

        protected override void Awake()
        {
            base.Awake();
            _inputActionAsset = Resources.Load<InputActionAsset>("InputActions");
            if (_inputActionAsset is null)
                throw new Exception("Inputactions not found, check if the file is in the resource folder under the correct name");
            
            
            //order in this list is the order of execution
            //before move, so it can subscribe to typing input
            _typingInput = new TypingInput(InputActionAsset);
            _uiInput = new UIInput(InputActionAsset);
            //before move, so interact clicks that are not found can still be cast into navigation clicks. 
            _cursor = new CursorInput(InputActionAsset);
            _interactInput = new InteractInput(InputActionAsset);
            _cameraInput = new CameraInput(InputActionAsset);
            _trackInputs = new TrackInputs(InputActionAsset);
            _playerMove = new PlayerMoveInput(InputActionAsset);
            _dialogInput = new DialogInput(InputActionAsset);
            _devInput = new DevInput(InputActionAsset);
            _emoteInput = new EmoteInput(InputActionAsset);
            _gamepadInput = new GamepadInput();
            _inputDetector = new InputDetector();
            Disposed = false;
        }

        protected void Start()
        {            
            PopulateActionMaps(InputActionAsset);
            
            _actionMapDictionary[ActionMaps.UI].Enable();
            _actionMapDictionary[ActionMaps.Interact].Enable();
            _actionMapDictionary[ActionMaps.Track].Enable();
            _actionMapDictionary[ActionMaps.PlayerMove].Enable();
            _actionMapDictionary[ActionMaps.Camera].Enable();
            _actionMapDictionary[ActionMaps.Emote].Enable();
            _actionMapDictionary[ActionMaps.Cursor].Enable();
            _actionMapDictionary[ActionMaps.Dev].Enable();
            _actionMapDictionary[ActionMaps.Typing].Enable();
            _actionMapDictionary[ActionMaps.Dialog].Disable();
        }
        
        protected void OnDestroy()
        {
            UIInput.Dispose();
            _uiInput = null;
            
            Cursor.Dispose();
            InteractInput.Dispose();
            PlayerMove.Dispose();
            EmoteInput.Dispose();
            CameraInput.Dispose();
            DialogInput.Dispose();
            DevInput.Dispose();
            TrackInputs.Dispose();
            GamepadInput.Dispose();
            TypingInput.Dispose();
            
            _cursor = null;
            _interactInput = null;
            _playerMove = null;
            _emoteInput = null;
            _cameraInput = null;
            _dialogInput = null;
            _devInput = null;
            _trackInputs = null;
            _gamepadInput = null;
            _typingInput = null;
            
            Disposed = true;
        }

        protected void Update()
        {
            _gamepadInput.Update();
        }

        public void PopulateActionMaps(InputActionAsset inputActionAsset)
        {
            _actionMapDictionary.Add(ActionMaps.UI, inputActionAsset.FindActionMap(ActionMaps.UI.ToString()));
            _actionMapDictionary.Add(ActionMaps.Interact, inputActionAsset.FindActionMap(ActionMaps.Interact.ToString()));
            _actionMapDictionary.Add(ActionMaps.PlayerMove, inputActionAsset.FindActionMap(ActionMaps.PlayerMove.ToString()));
            _actionMapDictionary.Add(ActionMaps.Emote, inputActionAsset.FindActionMap(ActionMaps.Emote.ToString()));
            _actionMapDictionary.Add(ActionMaps.Camera, inputActionAsset.FindActionMap(ActionMaps.Camera.ToString()));
            _actionMapDictionary.Add(ActionMaps.Cursor, inputActionAsset.FindActionMap(ActionMaps.Cursor.ToString()));
            _actionMapDictionary.Add(ActionMaps.Dialog, inputActionAsset.FindActionMap(ActionMaps.Dialog.ToString()));
            _actionMapDictionary.Add(ActionMaps.Dev, inputActionAsset.FindActionMap(ActionMaps.Dev.ToString()));
            _actionMapDictionary.Add(ActionMaps.Track, inputActionAsset.FindActionMap(ActionMaps.Track.ToString()));
            _actionMapDictionary.Add(ActionMaps.Typing, inputActionAsset.FindActionMap(ActionMaps.Typing.ToString()));
        }

        public void SetActionMapActive(ActionMaps actionMap, bool active)
        {
            if(active)
                _actionMapDictionary[actionMap].Enable();
            else
                _actionMapDictionary[actionMap].Disable();
        }

        public void SetUIInputActive(bool active)
        {
            _uiInputModule.enabled = active;
        }
    }
}