using System.Collections.Generic;

/// <summary>
/// Used to check if other privilege controllers exist on an object.
/// </summary>
public interface ICheckPrivilege
{
    public static Dictionary<int, List<ICheckPrivilege>> OBJECT_CHECKS = new Dictionary<int, List<ICheckPrivilege>>();
    
    public bool HasPrivilege { get; }
}
