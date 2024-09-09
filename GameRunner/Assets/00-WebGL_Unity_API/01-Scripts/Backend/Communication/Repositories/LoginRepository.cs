using System.Collections;
using Cohort.Ravel.Networking;
using Cohort.Ravel.Networking.Authorization;
using UnityEngine;
using UnityEngine.Events;

public class LoginRepository
{
    /// <summary>
    /// Try login user with password and email
    /// </summary>
    public IEnumerator TokenizedLogin(string loginToken, UnityAction<LoginRequest.ServerToken> onSuccess, UnityAction<string> onFailure)
    {
        RavelWebRequest req = LoginRequest.LoginTokenRequest(loginToken);
        yield return req.Send();

        RavelWebResponse res = new RavelWebResponse(req);
        LoginRequest.ServerToken serverToken;
        if (res.Success && res.TryGetData(out serverToken)) {
            onSuccess?.Invoke(serverToken);
        }
        else {
            onFailure?.Invoke(res.Error.FullMessage);
            Debug.Log(res.Error.FullMessage);
        }
    }
}
