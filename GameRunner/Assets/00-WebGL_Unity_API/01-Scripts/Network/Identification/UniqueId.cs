using System;
using Cohort.CustomAttributes;
using UnityEngine;

/// <summary>
/// Should be derived from MonoBehaviour to work.
/// </summary>
[Serializable]
public abstract class UniqueId : MonoBehaviour
{
	/// <summary>
	/// Identifying index, should always be -1 when not set. System will automatically assign indices, making sure all
	/// objects indices are unique.
	/// </summary>
	public int Identifier {
		get { return _id;}
		set {
			_id = value;
			_idSet = value >= 0;
		}
	}

	public bool IdentifierSet {
		get { return _idSet; }
	}

	[ReadOnly, SerializeField] private int _id = -1;
	private bool _idSet;
	
	public abstract string Name { get; }

	public virtual void OnValidate() {
		if (_id > 0 && !_idSet) {
			_id = -1;
		}
	}
}
