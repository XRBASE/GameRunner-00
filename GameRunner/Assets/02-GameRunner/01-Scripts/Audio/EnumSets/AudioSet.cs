using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Audioset that couples clips with states (enum values) so that they can easily be categorized and played.
/// </summary>
/// <typeparam name="TClip">ClipEntry class must be inherited for the enum values to be shown in the inspector.</typeparam>
/// <typeparam name="TState">Enum values with which the clips are being indexed.</typeparam>
public class AudioSet<TClip, TState> : ScriptableObject where TClip : ClipEntry<TState> where TState : Enum {
	[SerializeField] private List<TClip> _entries;
	
	public bool TryGetClip(TState state, out TClip clip) {
		for (int i = 0; i < _entries.Count; i++) {
			if (_entries[i].state.Equals(state)) {
				clip = _entries[i];
				return true;
			}
		}

		clip = null;
		return false;
	}
	
#if UNITY_EDITOR
	public bool CheckForDoubleEntries(out TState state) {
		for (int i = 0; i < _entries.Count; i++) {
			for (int j = i + 1; j < _entries.Count; j++) {
				if (_entries[i].state.Equals(_entries[j].state)) {
					state = _entries[i].state;
					return true;
				}
			}
		}

		state = default(TState);
		return false;
	}

	public void OnValidate() {
		if (CheckForDoubleEntries(out TState state)) {
			Debug.LogError($"Double state entry found for state: {state} found.\n These sounds will not be played.");
		}
	}
#endif
}

[Serializable]
public class ClipEntry<T> : ClipEntry where T : Enum {
	public T state;
}

public abstract class ClipEntry {
	public AudioClip audio;
	public bool loop;
	public bool oneShot;
}
