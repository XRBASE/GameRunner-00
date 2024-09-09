using Cohort.Ravel.Networking;

public class RoomRequest : TokenWebRequest
{
	private RoomRequest(Method method, string postfix) : base(method, "api/") {
		if (string.IsNullOrEmpty(postfix)) {
			_url += "room";
		}
		else {
			_url += "room/" + postfix;
		}
	}

	private RoomRequest(Method method, string postfix, string json) : base(method, "api/") {
		if (string.IsNullOrEmpty(postfix)) {
			_url += "room";
		}
		else {
			_url += "room/" + postfix;
		}

		_data = json;
	}

	/// <summary>
	/// Get roomdata based on roomid.
	/// </summary>
	public static RoomRequest GetRoom(string roomUUID) {
		return new RoomRequest(Method.Get, roomUUID);
	}
	
	/// <summary>
	/// Get room data based on space uuid.
	/// </summary>
	public static RoomRequest GetRoomBySpace(string spaceUUID) {
		string json = "{\"spaceId\": \"" + spaceUUID + "\"}";
		RoomRequest req = new RoomRequest(Method.PostJSON, "", json);

		return req;
	}
}
