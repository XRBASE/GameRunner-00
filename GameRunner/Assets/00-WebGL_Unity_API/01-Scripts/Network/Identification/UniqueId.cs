using Cohort.CustomAttributes;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Should be derived from MonoBehaviour to work.
/// </summary>
public abstract class UniqueId : MonoBehaviour
{
	/// <summary>
	/// Identifying index, should always be -1 when not set. System will automatically assign indices, making sure all
	/// objects indices are unique.
	/// </summary>
	public int Identifier {
		get { return _id;}
		set { _id = value; }
	}
	[ReadOnly, SerializeField] private int _id = -1;
	[HideInInspector, SerializeField] private bool _subscribed;
	
	public abstract string Name { get; }
	
	public virtual void OnValidate() {
#if UNITY_EDITOR
		if (_id >= 0 && EditorUtility.IsDirty(this) && IdLocator.IdTaken(this)) {
			_id = -1;
		}
#endif
	}
}
