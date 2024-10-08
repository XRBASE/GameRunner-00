using Cohort.Networking.PhotonKeys;
using Cohort.Networking.Players;
using ExitGames.Client.Photon;
using UnityEngine;

namespace Cohort.GameRunner.Players {
	public class LocalPlayer : Player {
		public string UUID { get; private set; }

		private void Awake() {
			IPlayer.Local = this;
		}

		private void Start() {
			ImportAvatar();
		}

		public override void OnJoinedRoom() {
			UUID = DataServices.Users.Local.id;
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