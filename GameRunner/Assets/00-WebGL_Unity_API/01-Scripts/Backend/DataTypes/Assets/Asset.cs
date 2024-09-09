using System;
using Cohort.Ravel.BackendData.Data;
using UnityEngine;

namespace Cohort.Ravel.Assets
{
    /// <summary>
    /// Baseclass which is used to contain asset data.
    /// This data can be used to download the asset's data.
    /// </summary>
    [Serializable]
    public class Asset : DataContainer
    {
        public override string Key {
            get { return id; }
        }

        public bool IsEmpty {
            get { return string.IsNullOrEmpty(id); }
        }

        public bool HasData {
            get { return _hasData; }
        }

        public byte[] Data {
            get { return _data; }
        }

        public FileType Type {
            get {
                if (file == null)
                    return FileType.none;
                if (_type == FileType.none) {
                    _type = FileExtensions.GetType(file.fileName);
                }
                return _type;
            }
        }
        private FileType _type = FileType.none;

        public string id;
        public string name;
        public string type;
        [SerializeField] private FileReference file;
        
        //both not serialized, so they're not shared over photon network.
        private bool _hasData = false;
        private byte[] _data; 
        
        public Asset(string id, string name) {
            this.id = id;
            this.name = name;
        }

        /// <summary>
        /// Sets the actual file binary, so it can later be loaded into the world.
        /// </summary>
        public void SetData(byte[] data) {
            _data = data;
        }

        public override bool Overwrite(DataContainer data) {
            if (data.GetType().IsSubclassOf(typeof(Asset))) {
                Asset ass = (Asset)data;
                bool hasChanges = false;

                if (ass.id != id) {
                    id = ass.id;
                    hasChanges = true;
                }

                if (ass.name != name) {
                    name = ass.name;
                    hasChanges = true;
                }

                if (ass.type != type) {
                    type = ass.type;
                    hasChanges = true;
                }

                if (ass.file != file) {
                    file = ass.file;
                    hasChanges = true;
                }

                return hasChanges;
            }
            
            throw GetOverwriteFailedException(data);
        }

        public static Asset Empty {
            get { return new Asset("", ""); }
        }

        public override string ToString() {
            return $"{name}";
        }
        
#region operators

        public static bool operator ==(Asset lhs, Asset rhs) {
            //check for null types
            if (lhs is null) {
                return rhs is null;
            } else if (rhs is null) {
                return false;
            }
            
            //if key matches, check if reference also matches
            if (lhs.Key == rhs.Key) {
                return lhs.file == rhs.file;
            }
            else {
                return false;
            }
        }
        
        public static bool operator !=(Asset lhs, Asset rhs) {
            return !(lhs == rhs);
        }
        
        public override bool Equals(object obj) {
            if (TryParse(obj, out Asset fr)) {
                return this == fr; 
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool TryParse(object obj, out Asset value) {
            value = null;
            
            if (obj is null) {
                return false;
            }

            if (obj is Asset) {
                value = (Asset)obj;
                return true;
            }
            return false;
        }
    
#endregion
    }

    [Serializable]
    public class FileReference
    {
        //not used:
        //public int fileSize;
        //public string fileType;
        
        public string fileName;
        public string fileDate;
        
#region operators

        public static bool operator ==(FileReference lhs, FileReference rhs) {
            if (lhs is null) {
                return rhs is null;
            }
            else if (rhs is null) {
                return false;
            }
            
            //reference matches if name and date both match.
            return lhs.fileName == rhs.fileName && lhs.fileDate == rhs.fileDate;
        }

        public static bool operator !=(FileReference lhs, FileReference rhs) {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj) {
            if (TryParse(obj, out FileReference fr)) {
                return this == fr; 
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool TryParse(object obj, out FileReference value) {
            value = null;
            
            if (obj is null) {
                return false;
            }

            if (obj is FileReference) {
                value = (FileReference)obj;
                return true;
            }
            return false;
        }
    
#endregion
    }
}