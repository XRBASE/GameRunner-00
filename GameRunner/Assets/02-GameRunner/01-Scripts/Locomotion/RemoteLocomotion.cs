using Cohort.GameRunner.AvatarAnimations;
using Cohort.Networking.PhotonKeys;
using ExitGames.Client.Photon;

namespace Cohort.GameRunner.LocoMovement {
	/// <summary>
	/// Remote locomotion class, that reads networked data and directs it as input in the Locomotion base class.
	/// </summary>
	public class RemoteLocomotion : Locomotion {
		public override ControlType Control {
			get { return ControlType.Remote; }
		}

		protected override void OnCustomPropertiesChanged(Hashtable changes, bool initialize) {
			_key = Keys.Get(Keys.Player.LocomotionState);
			if (changes.ContainsKey(_key)) {
				if ((changes[_key] != null) && (int)changes[_key] != (int)_sm.State) {
					if (_sm.State == State.Seat && (State)changes[_key] == State.Move) {
						((SeatState)_sm.Current).ExitState();
					}
					else {
						_sm.State = (State)changes[_key];
						_state = (State)changes[_key];
					}
				}
			}

			_key = Keys.Get(Keys.Player.Jump);
			if (changes.ContainsKey(_key)) {
				if (((CharAnimator.AnimationState)changes[_key] == CharAnimator.AnimationState.Jump ||
				    (CharAnimator.AnimationState)changes[_key] == CharAnimator.AnimationState.DoubleJump) &&
				    _sm.State == State.Move) {
					
					//check if it was not to long ago that the player in question jumped.
					float expire = (float)changes[Keys.Get(Keys.Player.JumpExpire)];
					if (expire >= TimeManager.Instance.RefTime &&
					    expire - TimeManager.Instance.RefTime < TimeManager.RESET_VALUE / 2f) {
						((MoveState)_sm.Current).Jump();
					}
				}
			}

			_key = Keys.Get(Keys.Player.Emote);
			if (changes.ContainsKey(_key)) {
				string guid = (string)changes[_key];
				float expire = (float)changes[Keys.Get(Keys.Player.EmoteExpire)];

				if (expire >= TimeManager.Instance.RefTime &&
				    expire - TimeManager.Instance.RefTime < TimeManager.RESET_VALUE / 2f) {
					Emote em = AnimationManager.Instance.GetEmote(guid);
					float nt = 1f - ((expire - TimeManager.Instance.RefTime) / em.clip.length);
					Animator.DoEmote(em, nt);
				}
			}
			
			_sm.Current.OnPlayerPropertiesChanged(changes, initialize);
		}
	}
}