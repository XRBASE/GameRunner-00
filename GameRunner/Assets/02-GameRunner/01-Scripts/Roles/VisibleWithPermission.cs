using Cohort.Permissions;

public class VisibleWithPermission : VisibleWithEnum<Permission>
{
    protected void Start()
    {
        SubscribeCheckValue(DataServices.Roles.Permissions, ref DataServices.Roles.onPermissionChanged);
    }
}
