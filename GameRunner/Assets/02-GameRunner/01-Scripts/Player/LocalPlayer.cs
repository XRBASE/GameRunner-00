using Cohort.Networking.PhotonKeys;
using Cohort.Networking.Players;
using ExitGames.Client.Photon;

namespace Cohort.GameRunner.Players {
	public class LocalPlayer : Player {
		private void Awake() {
			IPlayer.Local = this;
		}

		private void Start() {
			ImportAvatar();
		}

		public override void OnJoinedRoom() {
			_userName = DataServices.Users.Local.userName;
			_avatarUrl = DataServices.Users.Local.rpmAvatarUri;
			
			NetworkUserData();
		}

		protected virtual void NetworkUserData() {
			Hashtable changes = new Hashtable();
			changes.Add(Keys.Get(Keys.Player.Name), _userName);
			changes.Add(Keys.Get(Keys.Player.Avatar), _avatarUrl);

			SetCustomProperties(changes);
		}
	}
}