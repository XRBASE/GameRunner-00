using Cohort.Networking;

public class SessionRequest : TokenWebRequest
{
	private SessionRequest(Method method, string postfix) : base(method, "api/") {
		if (string.IsNullOrEmpty(postfix)) {
			_url += "session";
		}
		else {
			_url += "session/" + postfix;
		}
	}

	private SessionRequest(Method method, string postfix, string json) : base(method, "api/") {
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
	public static SessionRequest GetSession(string sessionId) {
		return new SessionRequest(Method.Get, $"{sessionId}/instance");
	}
}
