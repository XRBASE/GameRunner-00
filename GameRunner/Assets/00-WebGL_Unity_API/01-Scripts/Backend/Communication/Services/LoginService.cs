using Cohort.Ravel.Networking.Authorization;
using Cohort.Ravel.Users;
using UnityEngine;
using UnityEngine.Events;

public class LoginService : MonoBehaviour
{
    private LoginRepository _repo;

    public bool UserLoggedIn {
        get { return _loggedIn; }
    }

    private bool _loggedIn = false;

    public UnityAction<User> onUserLoggedIn;
    public UnityAction onUserLoggedOut;

    private void Awake()
    {
        _repo = new LoginRepository();
    }
    
    /// <summary>
    /// Log in using email and password.
    /// </summary>
    /// <param name="token">login token that has been created on the dashboard.</param>
    /// <param name="onSuccess">success called with access token.</param>
    /// <param name="onFailure">failure, called with error message.</param>
    public void TokenizedLogin(string token, UnityAction<LoginRequest.ServerToken> onSuccess = null, UnityAction<string> onFailure = null)
    {
        onSuccess += OnSystemsTokenRecieved;
        StartCoroutine(_repo.TokenizedLogin(token, onSuccess, onFailure));
    }

    /// <summary>
    /// Called as responce to recieving the accesstoken.
    /// </summary>
    /// <param name="accessToken">the access token.</param>
    public void OnSystemsTokenRecieved(LoginRequest.ServerToken token)
    {
        token.Decode();
        PlayerCache.SetString(LoginRequest.SYSTEMS_TOKEN_KEY, JsonUtility.ToJson(token));
        
        #if UNITY_EDITOR
        Debug.Log(token.token);
        #endif
        
        DataServices.Users.GetLocal(OnLoginSuccess);
    }

    /// <summary>
    /// Called when logging in using cached token, with local user as param. Invokes logged-in callback.
    /// </summary>
    /// <param name="local">local user that logged in.</param>
    public void OnLoginSuccess(User local)
    {
        _loggedIn = true;
        onUserLoggedIn?.Invoke(local);
    }

    /// <summary>
    /// Log out the currently logged in user. OnUserLoggedOut removes cached data.
    /// </summary>
    public void Logout()
    {
        onUserLoggedOut?.Invoke();
        PlayerCache.Clear();
        //load login scene
        
        //API_TODO: load the login scene
        //SceneManager.LoadScene(SceneManager.LOGIN_SCENE, LoadSceneMode.Single);
        
        _loggedIn = false;
    }
}
