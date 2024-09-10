using Cohort.Networking.PhotonKeys;
using Cohort.Networking.Players;
using ExitGames.Client.Photon;

namespace Cohort.GameRunner.Players {
	public class LocalPlayer : Player {
		private void Awake() {
			IPlayer.Local = this;
		}

		public override void OnJoinedRoom() {
			_userName = DataServices.Users.Local.userName;
			NetworkUserData();
		}

		protected virtual void NetworkUserData() {
			Hashtable changes = new Hashtable();
			changes.Add(Keys.Get(Keys.Player.Name), _userName);

			SetCustomProperties(changes);
		}
	}
}