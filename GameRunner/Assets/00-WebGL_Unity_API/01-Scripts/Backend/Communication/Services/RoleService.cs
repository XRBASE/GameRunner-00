using System;
using Cohort.Ravel.Permissions;
using Cohort.Ravel.Users;
using UnityEngine;

public class RoleService : MonoBehaviour
{
    public Action<Permission> onPermissionChanged;

    public Permission Permissions {
        get { return _permissions; }
    }
    
    private Permission _permissions;
    private RoleRepository _repo;

    private void Awake() {
        _repo = new RoleRepository();
        DataServices.Users.onLocalUserUpdated += UpdateLocalUserPermissions;
    }

    private void Start()
    {
        DataServices.Login.onUserLoggedOut += OnLogout;
    }

    private void OnDestroy()
    {
        DataServices.Login.onUserLoggedOut -= OnLogout;
    }

    /// <summary>
    /// Get all local permissions based on role array in the user class, place these rolls in the service class for
    /// enabling permissioned behaviours.
    /// </summary>
    /// <param name="onComplete">callback when action complete.</param>
    /// <param name="onFailure">callback for when action fails.</param>
    private void UpdateLocalUserPermissions(User local) {
        _repo.GetLocalPermissions(null, null);
    }
    
    /// <summary>
    /// Sets the role and permission values and calls update actions.
    /// </summary>
    public void SetRoleAndPermissions(Permission permissions) {
        _permissions = permissions;
        
        onPermissionChanged?.Invoke(permissions);
    }

    /// <summary>
    /// Called from the logout callback, removes existing permissions and auth 
    /// </summary>
    private void OnLogout() {
        SetRoleAndPermissions(Permission.None);
    }
}
