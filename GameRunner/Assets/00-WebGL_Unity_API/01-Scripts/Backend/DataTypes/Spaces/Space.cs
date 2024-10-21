using System;
using Cohort.Assets;
using Cohort.BackendData;

namespace Cohort.Spaces
{
    [Serializable]
    public class Space : DataContainer
    {
        public override string Key {
            get { return id; }
        }

        public string id;
        public string name;
        public string description;
        public string type;
        public string domain;
        
        public Environment environment;
        public FileSlot[] slots;

        //used for faking spaces for test purposes.
        public bool isProxy = false;

        public Space() { }
        
        /// <summary>
        /// Create empty space, just containing uuid, so Repo/Service calls can fill the other data of this class.
        /// </summary>
        /// <param name="uuid">uuid of the space.</param>
        public Space(string uuid)
        {
            id = uuid;
        }
        
        /// <summary>
        /// Tries to retrieve the asset that matches the given slot name.
        /// </summary>
        /// <param name="slotName">Name of the slot, for which the asset is being retrieved.</param>
        /// <param name="asset">Out variable for the asset, if any was found.</param>
        /// <returns></returns>
        public bool TryGetSlotAsset(string slotName, out Asset asset) {
            for (int i = 0; i < slots.Length; i++) {
                if (slots[i].name == slotName) {
                    asset = slots[i].asset;
                    return true;
                }
            }

            asset = null;
            return false;
        }
        
        /// <summary>
        /// Sets the value of the slot, to the given assets.
        /// </summary>
        /// <param name="slotName">slot name/id to which the asset is set.</param>
        /// <param name="asset">Asset that is being assigned.</param>
        public void SetSlotAsset(string slotName, Asset asset) {
            for (int i = 0; i < slots.Length; i++) {
                if (slots[i].name == slotName) {
                    slots[i].asset = asset;
                }
            }
        }
        
        public override bool Overwrite(DataContainer data)
        {
            if (data is Space) {
                Space s = (Space)data;
                bool hasChanges = false;
                
                if (!string.IsNullOrEmpty(s.name)) {
                    name = s.name;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(s.id)) {
                    id = s.id;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(s.description)) {
                    description = s.description;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(s.type)) {
                    type = s.type;
                    hasChanges = true;
                }
                isProxy = s.isProxy;

                if (s.environment != null) {
                    environment.Overwrite(s.environment);
                    hasChanges = true;
                }

                if (s.slots != null) {
                    slots = s.slots;
                }

                return hasChanges;
            }

            throw GetOverwriteFailedException(data);
        }

        public override string ToString()
        {
            return $"SPACE_{name}({environment.name})";
        }

        public enum Type {
            Unknown = -1,
            Public,
            Domain,
            Dev,
        }
    }
}