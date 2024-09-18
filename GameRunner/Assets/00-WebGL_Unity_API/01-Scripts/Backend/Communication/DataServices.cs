using Cohort.Ravel.BackendData.Data;
using Cohort.Ravel.BackendData.DataService;
using Cohort.Ravel.BackendData.Images;
using Cohort.Patterns;
using UnityEngine;

//before other service classes
[DefaultExecutionOrder(-1)]
public class DataServices : Singleton<DataServices>
{
    //TODO: replace this with a service locator pattern maybe?
    public static LoginService Login {
        get { return _login; }
    }
    
    public static UserService Users {
        get { return _users; }
    }
    
    public static RoleService Roles {
        get { return _roles; }
    }
    
    public static AssetService Assets {
        get{
            return _assets;
        }
    }
    
    public static ImageService Images {
        get { return _images; }
    }

    public static StorageService Storage {
        get { return _storage; }
    }

    public static PhotonService Photon {
        get { return _photon; }
    }

    private static LoginService _login;
    private static UserService _users;
    private static RoleService _roles;
    private static AssetService _assets;
    private static ImageService _images;
    private static StorageService _storage;
    private static PhotonService _photon;
    
    protected override void Awake()
    {
        base.Awake();
        
        //add services to object
        _login = gameObject.AddComponent<LoginService>();
        _users = gameObject.AddComponent<UserService>();
        _images = gameObject.AddComponent<ImageService>();
        _assets = gameObject.AddComponent<AssetService>();
        _storage = gameObject.AddComponent<StorageService>();
        _roles = gameObject.AddComponent<RoleService>();
        _photon = gameObject.AddComponent<PhotonService>();
    }
}
