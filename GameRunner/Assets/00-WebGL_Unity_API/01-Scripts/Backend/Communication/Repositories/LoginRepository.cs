using Cohort.Networking.Authorization;
using Cohort.Networking;

using System.Collections;
using UnityEngine.Events;
using UnityEngine;

public class LoginRepository
{
    /// <summary>
    /// Try login user with password and email
    /// </summary>
    public IEnumerator TokenizedLogin(string loginToken, UnityAction<LoginRequest.SimpleToken> onSuccess, UnityAction<string> onFailure)
    {
        RavelWebRequest req = LoginRequest.LoginTokenRequest(loginToken);
        yield return req.Send();

        RavelWebResponse res = new RavelWebResponse(req);
        LoginRequest.SimpleToken serverToken;
        if (res.Success && res.TryGetData(out serverToken)) {
            onSuccess?.Invoke(serverToken);
        }
        else {
            onFailure?.Invoke(res.Error.FullMessage);
            Debug.Log(res.Error.FullMessage);
        }
    }
}
