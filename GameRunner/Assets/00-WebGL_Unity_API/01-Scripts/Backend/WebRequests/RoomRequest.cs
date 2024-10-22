using Cohort.Networking;

public class RoomRequest : TokenWebRequest
{
	private RoomRequest(Method method, string postfix) : base(method, "api/") {
		if (string.IsNullOrEmpty(postfix)) {
			_url += "session";
		}
		else {
			_url += "session/" + postfix + "/instance";
		}
	}

	private RoomRequest(Method method, string postfix, string json) : base(method, "api/") {
		if (string.IsNullOrEmpty(postfix)) {
			_url += "session";
		}
		else {
			_url += "session/" + postfix;
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
