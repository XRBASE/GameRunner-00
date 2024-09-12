using System;
using System.Collections.Generic;
using Cohort.CustomAttributes;
using Cohort.Ravel.Tools.Timers;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Cohort.GameRunner.AvatarAnimations {
    public class CharAnimator : MonoBehaviour {
        //emote clip names
        private const string FULL_BODY_EMOTE_NAME = "EM_Fullbody";
        private const string UPPER_BODY_EMOTE_NAME = "EM_Upperbody";
		
        //parameter hashes
        private static readonly int STATE = Animator.StringToHash("State");
        private static readonly int SPEED = Animator.StringToHash("Speed");
        private static readonly int GENDER = Animator.StringToHash("Gender");
        private static readonly int EMOTE_ESC = Animator.StringToHash("EmoteEscape");
        
        public AnimationState State {
            get { return _state; }
        }
        [ReadOnly, SerializeField] private AnimationState _state;

        //currently available emotes (key is matched to emote's index in the list).
        public EmoteSet emoteSet;

        public AnimatorOverrideController RuntimeOverride {
            get { return _runtimeOverride; }
            set { _runtimeOverride = value; }
        }

        //should be available on avatar
        private Animator _animator;
        //provided in AnimationMananger class
        private RuntimeAnimatorController _runtimeController;
        //created runtime, used for swapping the emotes
        private AnimatorOverrideController _runtimeOverride;
        //used to store a singular emote swap
        private List<KeyValuePair<AnimationClip, AnimationClip>> _override;
        private Timer _upperEmoteCooldown;
        
        public void CopyFrom(CharAnimator copy) {
			if (copy == null)
				return;

			AnimationState state = copy.State;
			float t = copy._animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

			_runtimeOverride = copy._runtimeOverride;
			_animator.runtimeAnimatorController = _runtimeOverride;
			_upperEmoteCooldown = copy._upperEmoteCooldown;
			emoteSet = copy.emoteSet;
			
			_animator.SetFloat(SPEED, copy._animator.GetFloat(SPEED));
			ForceEnterState(state, t);
		}
		
		public void Initialize(float gender) {
			//directly sets it to finished
			_upperEmoteCooldown = new Timer(0f, true);

			if (emoteSet == null) {
				emoteSet = AnimationManager.Instance.DefaultEmotes;
			}

			_animator = gameObject.GetComponent<Animator>();
			_animator.applyRootMotion = false;

			if (_animator.runtimeAnimatorController != null) {
				_runtimeController = _animator.runtimeAnimatorController;
			}
			else {
				_runtimeController = AnimationManager.Instance.AnimationController;
				_animator.runtimeAnimatorController = _runtimeController;
			}
			_runtimeOverride = new AnimatorOverrideController(_runtimeController);
			
			_animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
			_animator.SetFloat(GENDER, gender);

			SetState((int)AnimationState.Idle, true);
			
			_override = new List<KeyValuePair<AnimationClip, AnimationClip>>();
		}

		/// <summary>
		/// Returns a bone of the human body, given in this animator and matching the currently selected avatar.
		/// </summary>
		public Transform GetBone(HumanBodyBones bone) {
			return _animator.GetBoneTransform(bone);
		}

		/// <summary>
		/// Same as set state, but made specifically for animator events, so we can better track them if any unexpected
		/// behaviour is encountered in development.
		/// </summary>
		/// <param name="state">new state to go into.</param>
		public void EventSetState(int state) {
			//if you ever need to know what state called this event
			//(sometimes it shows the previous clip, if this is called during transition)
			//Debug.LogError($"EVT state: {(AnimationState)state} by {_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name}");
			
			SetState(state, true);
		}

		/// <summary>
		/// Changes animator state into new state that is passed.
		/// </summary>
		/// <param name="newState">New state the animator should enter into.</param>
		/// <returns>True/False state can (and will) be entered</returns>
		public bool SetState(int newState, bool force) {
			return SetState((AnimationState)newState, force);
		}
		
		/// <summary>
		/// Changes animator state into new state that is passed.
		/// </summary>
		/// <param name="newState">New state the animator should enter into.</param>
		/// <param name="force">Should the can enter state check be skipped when making this call.
		/// this call still uses the constraints of the animator.</param>
		/// <returns>True/False state can (and will) be entered</returns>
		public bool SetState(AnimationState newState, bool force = false) {
			if (force || CanEnterState(newState)) {
				_state = newState;
				_animator.SetInteger(STATE, (int)newState);
				return true;
			}
			else {
				return false;
			}
		}
		
		/// <summary>
		/// (Unsafe) forcefully enter state without taking account of the animator constraints. The state will always be entered. 
		/// </summary>
		/// <param name="state">Animation state to enter</param>
		/// <param name="normalizedTime">Normalized time with which the state will be entered.</param>
		public void ForceEnterState(AnimationState state, float normalizedTime = 0f) {
			ForceEnterState(state.ToString(), normalizedTime);
		}

		private void ForceEnterState(string state, float normalizedTime = 0f, int layer = 0) {
			if (layer == 0) {
				_state = (AnimationState)Enum.Parse(typeof(AnimationState), state, false);
				_animator.SetInteger(STATE, (int)_state);
			}
			
			_animator.Play(state, layer, normalizedTime);
		}
		
		/// <summary>
		/// Force the emote to stop playing and enter the idle state as result.
		/// </summary>
		public void ForceExitEmote() {
			_animator.SetTrigger(EMOTE_ESC);
		}

		/// <summary>
		/// True/False can I, given the animator constraints, enter this state right now?
		/// </summary>
		public bool CanEnterState(AnimationState state) {
			switch (state) {
				case AnimationState.Idle:
					return _state 
						is AnimationState.Moving 
						or AnimationState.Land 
						or AnimationState.DoubleLand  
						or AnimationState.FallDown
						or AnimationState.StandUp
						or AnimationState.EmoteFullBody
						or AnimationState.Teleport;
				case AnimationState.Moving:
					return _state
						is AnimationState.Idle
						or AnimationState.Land
						or AnimationState.DoubleLand
						or AnimationState.FallDown
						or AnimationState.StandUp
						or AnimationState.EmoteFullBody;
				case AnimationState.Jump:
					return _state
						is AnimationState.Idle
						or AnimationState.Moving
						or AnimationState.Jump
						or AnimationState.Fall
						or AnimationState.Land
						or AnimationState.DoubleLand
						or AnimationState.EmoteFullBody;
				case AnimationState.Fall:
					//jump auto sets state to fall when finished
					return !(_state is AnimationState.Jump or AnimationState.DoubleJump);
				case AnimationState.Land:
				case AnimationState.DoubleLand:
					return _state
						is AnimationState.Jump
						or AnimationState.Fall
						or AnimationState.DoubleFall
						or AnimationState.DoubleJump;
				case AnimationState.DoubleFall:
					return _state
						is AnimationState.DoubleJump;
				case AnimationState.FallDown:
					return _state
						is AnimationState.Fall;
				case AnimationState.SitDown:
					return true;
				case AnimationState.Sitting:
					return _state
						is AnimationState.SitDown;
				case AnimationState.StandUp:
					return _state
						is AnimationState.Sitting
						or AnimationState.SitDown;
				case AnimationState.EmoteFullBody:
					return !(_state
						is AnimationState.Jump
						or AnimationState.Fall
						or AnimationState.DoubleFall
						or AnimationState.DoubleJump
						or AnimationState.FallDown
						or AnimationState.SitDown
						or AnimationState.Sitting
						or AnimationState.StandUp);
				case AnimationState.Teleport:
				case AnimationState.EmoteHalfBody:
					return true;
				case AnimationState.DoubleJump:
					return _state
						is AnimationState.Jump
						or AnimationState.Fall;
				default:
					return false;
			}
		}

		/// <summary>
		/// Make avatar play emote animation.
		/// </summary>
		/// <param name="emoteId">index of emote in emoteset.</param>
		/// <returns>True/False can play emote.</returns>
		public bool DoEmote(int emoteId) {
			return DoEmote(emoteId, out Emote em);
		} 
		
		/// <summary>
		/// Call for executing emote (both upper- and fullbody)
		/// </summary>
		/// <param name="emoteId">index of emote in emote set.</param>
		/// <returns>True/False possible to execute emote</returns>>
		public bool DoEmote(int emoteId, out Emote emote) {
			emote = null;
			if (!emoteSet.TryGetEmote(emoteId, out emote))
				return false;

			return DoEmote(emote);
		}

		/// <summary>
		/// Make avatar do an emote, based on its guid.
		/// </summary>
		/// <param name="guid">guid index of emote in the animationManager dictionary.</param>
		/// <returns>True/False emote can be played.</returns>
		public bool DoEmote(string guid) {
			Emote emote = AnimationManager.Instance.GetEmote(guid);

			return DoEmote(emote);
		}
		
		/// <summary>
		/// Make avatar do an emote.
		/// </summary>
		/// <param name="emote">emote to execute</param>
		/// <param name="normalizedTime">normalized time at which to play the emote.</param>
		/// <returns>True/False can play emote.</returns>
		public bool DoEmote(Emote emote, float normalizedTime = 0f) {
			bool enter = CanEnterState(AnimationState.EmoteFullBody);
			if (emote.isFullBody && !enter) 
				return false;
			if (!emote.isFullBody && !_upperEmoteCooldown.HasFinished || !(enter || _state == AnimationState.Sitting))
				return false;
			
			ReplaceClip((emote.isFullBody) ? FULL_BODY_EMOTE_NAME : UPPER_BODY_EMOTE_NAME, emote.clip);
			
			if (emote.isFullBody) {
				ForceEnterState(AnimationState.EmoteFullBody, normalizedTime);
			}
			else {
				_upperEmoteCooldown = new Timer(emote.clip.length, true);
				ForceEnterState("EmoteUpperBody", normalizedTime, 1);
			}

			return true;
		}
		
		/// <summary>
		/// Set animator speed value for moving characters.
		/// </summary>
		/// <param name="speed">Current normalized speed (1f for running, 0.5 for walk, 0 for idle).</param>
		public void SetSpeed(float speed) {
			_animator.SetFloat(SPEED, speed);
		}

		/// <summary>
		/// Overrides the emote animation clip with the one given in the emote set.
		/// </summary>
		/// <param name="originalName">Name of the original emote clip.</param>
		/// <param name="clip">New clip with which the old one will be replaced.</param>
		private void ReplaceClip(string originalName, AnimationClip clip) {
			AnimationClip originalClip = null;
			for (int i = 0; i < _runtimeController.animationClips.Length; i++) {
				if (_runtimeController.animationClips[i].name == originalName) {
					originalClip = _runtimeController.animationClips[i];
				}
			}

			_override.Clear();
			_override.Add(new KeyValuePair<AnimationClip, AnimationClip>(originalClip, clip));
			
			_runtimeOverride.ApplyOverrides(_override);
			_animator.runtimeAnimatorController = _runtimeOverride;
		}
		
		public enum AnimationState {
			Unknown = -1,
			Idle = 0,
			Moving = 1,
			Jump = 2,
			Fall = 3, //Set through animation event
			Land = 4,
			DoubleFall = 5, //after double jump
			FallDown = 6,
			SitDown = 7,
			Sitting = 8, //Set through animation event
			StandUp = 9,
			EmoteFullBody = 10, //no upperbody, as these emotes are executed during another animation event.
			Teleport = 11,
			DoubleJump = 12,
			DoubleLand = 13,
			EmoteHalfBody = 14,
		}

#if UNITY_EDITOR
		[CustomEditor(typeof(CharAnimator))]
		private class CharAnimatorEditor : Editor {
			private CharAnimator _instance;
			
			private float _gender;
			private AnimationState _state;
			private int _emote = 0;

			private void OnEnable() {
				_instance = (CharAnimator)target;
				if (Application.isPlaying) {
					_instance.Initialize(_gender);
				}
			}

			public override void OnInspectorGUI() {
				DrawDefaultInspector();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Test functions");
				
				EditorGUI.BeginChangeCheck();
				_gender = EditorGUILayout.Slider("gender", _gender, 0, 1);
				if (EditorGUI.EndChangeCheck() && Application.isPlaying) {
					_instance._animator.SetFloat(GENDER, _gender);
				}
				
				_state = (AnimationState) EditorGUILayout.EnumPopup("state", _state);
				_emote = Mathf.RoundToInt(EditorGUILayout.Slider("emote", _emote, 0, 5));

				if (Application.isPlaying && GUILayout.Button("Update")) {
					_instance.ForceEnterState(_state);
				}
				if (Application.isPlaying && GUILayout.Button("Emote")) {
					_instance.DoEmote(_emote);
				}
			}
		}
#endif
    }
}