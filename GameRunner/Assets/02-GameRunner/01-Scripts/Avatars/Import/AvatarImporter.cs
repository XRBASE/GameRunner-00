using System;
using System.Collections.Generic;
using Cohort.Patterns;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
    public class AvatarImporter : Singleton<AvatarImporter> {
        public const string AVATAR_OBJ_NAME = "avatar";
        public const int PLAYER_LAYER = 6;

        [SerializeField] private SceneAvatar _templateAvatar;

        private Dictionary<string, Avatar> _avatarLibrary = new Dictionary<string, Avatar>();

#if UNITY_EDITOR
        //editor debug values
        [SerializeField, HideInInspector] private ImportData.AvatarType _createAvatarType;
        [SerializeField, HideInInspector] private bool _overrideData;
        [SerializeReference] private ImportData _data;
#endif
	    
	    public void ImportTemplate(Action<Avatar> onAvatarImported, Transform parent = null) {
		    if (_avatarLibrary.ContainsKey(_templateAvatar.Data.GUID)) {
			    onAvatarImported?.Invoke(CopyFromLibrary(_templateAvatar.Data.GUID, parent));
			    return;
		    }
            
		    ImportAction action = ImportAction.GetAction(_templateAvatar.Data);
		    action.Execute(onAvatarImported, parent);
	    }
	    
	    /// <summary>
	    /// Imports new avatar using given data description.
	    /// </summary>
	    /// <param name="photonData">Saved photon avatar data string.</param>
	    /// <param name="onAvatarImported">Callback for when avatar has been imported.</param>
	    public void ImportAvatar(string photonData, Action<Avatar> onAvatarImported, Transform parent = null) {
		    /*if (!AvatarData.TryGetData(photonData, out AvatarData data)) {
			    Debug.LogError($"Could not process avatar data for user: {name}!");
			    return;
		    }*/
		    
		    //TODO: replace with code for both rpm
		    AvatarData data = new AvatarData(photonData, ImportData.AvatarType.ReadyPlayerMe, 0.0f); 
		    
		    ImportAvatar(data.GetData(), onAvatarImported, parent);
	    }

	    /// <summary>
	    /// Imports new avatar using given data description.
	    /// </summary>
	    /// <param name="data">Data description of the avatar being created.</param>
	    /// <param name="onAvatarImported">Callback for when avatar has been imported.</param>
	    public void ImportAvatar(ImportData data, Action<Avatar> onAvatarImported, Transform parent = null) {
#if UNITY_EDITOR
		    if (_overrideData) {
			    data = _data;
		    }
#endif
		    if (_avatarLibrary.ContainsKey(data.GUID)) {
			    onAvatarImported?.Invoke(CopyFromLibrary(data.GUID, parent));
			    return;
		    }
            
		    ImportAction action = ImportAction.GetAction(data);
		    action?.Execute(onAvatarImported, parent);
	    }
	    
	    /// <summary>
	    /// Imports new avatar using given data description.
	    /// </summary>
	    /// <param name="data">data description of the avatar being created.</param>
	    public void ImportAvatar(ImportData data , Transform parent = null) {
		    ImportAction action = ImportAction.GetAction(data);
            
		    action?.Execute(null, parent);
	    }
	    
	    /// <summary>
	    /// Callback that is fired whenever the avatar has been created.
	    /// </summary>
	    /// <param name="avatar">Avatar that has been created.</param>
	    public void AddToLibrary(Avatar avatar) {
		    _avatarLibrary[avatar.GUID] = avatar;
	    }
	    
	    /// <summary>
	    /// Removes specific avatar from the preloaded library.
	    /// </summary>
	    /// <param name="avatar">Avatar that needs to be removed from the library</param>
	    public void RemoveFromLibrary(Avatar avatar) {
		    _avatarLibrary.Remove(avatar.GUID);
	    }
        
	    /// <summary>
	    /// Copy an existing avatar from the avatar library. This is only used for custom avatars and not for loaded RPM avatars.
	    /// </summary>
	    /// <param name="guid">guid of the loaded avatar.</param>
	    /// <param name="parent">parent gameobject to which the new avatar will be parented.</param>
	    private Avatar CopyFromLibrary(string guid, Transform parent = null) {
		    return _avatarLibrary[guid].Copy(parent);
	    }
    }
}