using Cohort.BackendData.Images;
using Cohort.Assets;
using Cohort.Spaces;

using UnityEngine;

using Space = Cohort.Spaces.Space;

/// <summary>
/// Internal version of space, for use outside of the backend e.g. personal room or scene testing.
/// </summary>
public class SpaceReference : ScriptableObject
{
    public new string name;
    public string description;

    public string sceneName;
    public Sprite sceneImage;
    
    private string id;
    
    //PHOE: Make proper function to load the local assetbundle.
    
    public Space GetSpace()
    {
        Space space = new Space();
        space.name = name;
        space.id = "PROXY_" + System.Guid.NewGuid();
        space.description = description;
        space.isProxy = true;
        
        space.environment = new Environment();
        space.environment.name = sceneName;
        
        if (sceneImage != null) {
            space.environment.previewImage = new Image(space.id, space.name);
            space.environment.previewImage.SetImage(sceneImage, ImageSize.I1920);
        }
        return space;
    }

    #region Operators
    public static bool operator ==(SpaceReference lhs, SpaceReference rhs)
    {
        if (lhs is null || rhs is null) {
            return (lhs is null && rhs is null);
        }

        return lhs.id == rhs.id;
    }

    public static bool operator !=(SpaceReference lhs, SpaceReference rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator ==(SpaceReference lhs, Space rhs)
    {
        if (lhs is null || rhs is null) {
            return (lhs is null && rhs is null);
        }

        return lhs.id == rhs.id;
    }

    public static bool operator !=(SpaceReference lhs, Space rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator ==(Space lhs, SpaceReference rhs)
    {
        return rhs == lhs;
    }

    public static bool operator !=(Space lhs, SpaceReference rhs)
    {
        return !(lhs == rhs);
    }
    
    public override bool Equals(object o)
    {
        if (o.GetType() == typeof(SpaceReference)) {
            return (SpaceReference) o == this;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    #endregion
}
