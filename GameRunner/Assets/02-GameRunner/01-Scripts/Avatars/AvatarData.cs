using System;
using UnityEngine;

namespace Cohort.GameRunner.Avatars {
    /// <summary>
    /// Data object that is used to network the avatar of a player. This object contains all data required to build any avatar. 
    /// </summary>
    [Serializable]
    public class AvatarData {
        //right now RPM only, url from which model can be retrieved
        public string url;
        //identifier from which prefab models can be retrieved using the avatar imported library.
        public string guid;
        //gender of the avatar 0 fem, 1 masc.
        public float gender;
        //what kind of avatar is described in this data.
        [SerializeField] private ImportData.AvatarType type;
        
        public AvatarData(string url, ImportData.AvatarType type, float gender) {
            guid = Guid.NewGuid().ToString();
            this.url = url;
            this.type = type;
            this.gender = gender;
        }

        public AvatarData(string url, ImportData.AvatarType type, string guid, float gender) {
            this.guid = guid;
            this.url = url;
            this.type = type;
            this.gender = gender;
        }

        /// <summary>
        /// Returns import data based on the network data, so it can in term be entered into the imported.
        /// </summary>
        public ImportData GetData() {
            switch (type) {
                case ImportData.AvatarType.ReadyPlayerMe:
                    return new RPMImportData(url, guid, gender);
                case ImportData.AvatarType.Prefab:
                    return new PrefabImportData(url, guid, gender);
                case ImportData.AvatarType.Glb:
                    return new GLBImportData(guid, gender);
                default:
                    Debug.LogError($"Avatar import: Missing type implementation {type}");
                    break;
            }
            return null;
        }
    
        /// <summary>
        /// Try to convert a string (json) into an instance of this class.
        /// </summary>
        /// <param name="json">json data of this instance.</param>
        /// <param name="data">result output variable</param>
        /// <returns>True/False system was able to create ravatar data of the provided input string.</returns>
        public static bool TryGetData(string json, out AvatarData data) {
            if (!string.IsNullOrEmpty(json)) {
                data = JsonUtility.FromJson<AvatarData>(json);

                return data != null;
            }
        
            data = null;
            return false;
        }
    }
}