using Cohort.Networking.Users;
using Cohort.Networking;
using Cohort.Users;

using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System;

public class UserRepository : DataRepository<User>
{
    /// <summary>
    /// Create user data repo, modules use data cashe
    /// </summary>
    public UserRepository() :base(true, new TimeSpan(0,0,1,0)) { }

    /// <summary>
    /// Retrieve local user data.
    /// </summary>
    public IEnumerator RetrieveLocal(UnityAction<User> onComplete, UnityAction<string> onFailure)
    {
        User local = null;
        if (!TryGetFromCache(DataServices.Users.Local, out local)) {
            UserRequest req = UserRequest.GetMe();
            yield return req.Send();

            RavelWebResponse res = new RavelWebResponse(req);
            if (!res.Success) {
                Debug.LogError(res.Error.FullMessage);
                onFailure?.Invoke(res.Error.FullMessage);
                yield break;
            }

            if (!res.TryGetData(out local)){
                Debug.LogError($"Exception parsing player data: {res.Error.FullMessage}!");
                onFailure?.Invoke($"Session expired, please log in again!\n{res.Error.FullMessage}");
                yield break;
            }
        }
        
        //PHOE: Implement user images
        //local.profileImage.TryGetImage(ImageSize.I1920, out Sprite image);

        DataServices.Users.Local = local;
        onComplete?.Invoke(local);
    }
    
    public IEnumerator SetLocalUserAvatarData(string avatarData, Action onComplete)
    {
        Debug.LogError("Avatardata dispabled because of RPM implementation");
        yield return null;
        
        /* AVATR: Fix or remove the avatar panel.
        RavelWebRequest req = UserRequest.UpdateAvatarUrl(avatarData);
        yield return req.Send();
        RavelWebResponse res = new RavelWebResponse(req);
        
        if (!res.Success) {
            Debug.LogError(res.Error.FullMessage);
        }
        else {
            DataServices.Users.Local.avatarData = avatarData;
            onComplete?.Invoke();
        }*/
    }
}
