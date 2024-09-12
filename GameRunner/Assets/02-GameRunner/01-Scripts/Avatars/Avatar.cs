using Cohort.GameRunner.AvatarAnimations;
using MathBuddy.MonoBehaviours;
using MathBuddy.Transforms;
using UnityEngine;
using UAvatar = UnityEngine.Avatar;

namespace Cohort.GameRunner.Avatars {
	public class Avatar : MonoBehaviour {
		/// <summary>
		/// Unique string to identify this avatar with, used to check if a new avatar needs to be spawned.
		/// </summary>
		public string GUID { get; private set; }

		/// <summary>
		/// Feet position of the avatar.
		/// </summary>
		public Transform Feet { get; private set; }

		/// <summary>
		/// Head position of the avatar. 
		/// </summary>
		public Transform Head { get; private set; }

		private CharAnimator _animator;
		private UAvatar _avatar;
		//Gender is a spectrum
		private float _gender;

		//used for enabling/disabling the avatar.
		private Renderer[] _renderers;

		/// <summary>
		/// This sets up the gameobject and animator and finds all renderers.
		/// </summary>
		/// <param name="gender">avatar gender, 0 for feminine, 1 for masculine.</param>
		/// <param name="guid">unique identifier for avatar.</param>
		public void Build(float gender, string guid, EmoteSet emoteOverride = null) {
			gameObject.name = AvatarImporter.AVATAR_OBJ_NAME;
			gameObject.transform.SetLayer(AvatarImporter.PLAYER_LAYER);

			GUID = guid;
			_gender = gender;

			_animator = gameObject.GetOrAddComponent<CharAnimator>();

			if (emoteOverride != null) {
				_animator.emoteSet = emoteOverride;
			}

			_animator.Initialize(_gender);
			_renderers = GetComponentsInChildren<Renderer>();
		}

		/// <summary>
		/// Enable/Disable visibility for the avatar object.
		/// </summary>
		public void SetVisible(bool enabled) {
			for (int i = 0; i < _renderers.Length; i++) {
				_renderers[i].enabled = enabled;
			}
		}

		/// <summary>
		/// Copies head and feet helpers, so their references are retained.
		/// </summary>
		/// <param name="copy"></param>
		public void SetupHelpers(Avatar copy) {
			if (copy == null) {
				Head = new GameObject("Helper_Head").transform;
				Head.SetParent(_animator.GetBone(HumanBodyBones.Head), false);

				Feet = new GameObject("Helper_Feet").transform;
				Feet.SetParent(transform, false);
			}
			else {
				Head = copy.Head;
				Head.SetParent(_animator.GetBone(HumanBodyBones.Head), false);

				Feet = copy.Feet;
				Feet.SetParent(transform, false);
			}
		}

		/// <summary>
		/// Copy this avatar object into a new instance.
		/// </summary>
		/// <param name="parent">object to which the new object will be parented (position of this object will be used for avatar position).</param>
		public Avatar Copy(Transform parent) {
			Avatar copy = Instantiate(this, parent);
			copy.Build(_gender, GUID);
			copy.gameObject.SetActive(true);

			return copy;
		}
	}
}