using Cohort.BackendData;

using UnityEngine;
using System;

namespace Cohort.Ravel.Networking.Files
{
    [Serializable]
    public class File : DataContainer
    {
        public string Name { get { return fileName; } }
        
        public string Id { get { return id; } }
        
        public override string Key { get { return id; } }
        
        public byte[] Data { get { return _data; } }

        /// <summary>
        /// Should file service download the file byte array
        /// This is not used for video's as they're streamed from the presigned url.
        /// </summary>
        public bool RequiresDownload {
            get {
                //checks if filetype is not any of the given types
                return (Type & FileType.mp4) == 0;
            }
        }
        
        public FileType Type {
            get { return FileExtensions.GetType(fileName); }
        }
        
        public bool IsEmpty {
            get { return string.IsNullOrEmpty(id); }
        }
        
        public bool HasData {
            get { return (_data != null && _data.Length > 0); }
        }

        /// <summary>
        /// Direct download url for file, only set during downloading process. Used for streaming filedata (videos).
        /// </summary>
        public string PresignedUrl {
            get { return _presignedUrl; }
        }

        [SerializeField] private string id;
        [SerializeField] private string fileName;

        //not serialized to prevent big jsons
        private byte[] _data;
        private string _presignedUrl;

        public File(string id, string fileName)
        {
            this.id = id;
            this.fileName = fileName;
        }

        public void OnDataDownloaded(byte[] data, string presignedUrl)
        {
            _data = data;
            _presignedUrl = presignedUrl;
        }
        
        public override bool Overwrite(DataContainer data)
        {
            if (data.GetType() == typeof(File)) {
                File other = (File) data;
                bool hasChanges = false;
                
                if (!string.IsNullOrEmpty(other.id)) {
                    id = other.id;
                    hasChanges = true;
                }

                if (!string.IsNullOrEmpty(other.fileName)) {
                    fileName = other.fileName;
                    hasChanges = true;

                }

                if (other._data != null) {
                    this._data = other._data;
                    hasChanges = true;
                }

                return hasChanges;
            }

            throw GetOverwriteFailedException(data);
        }

        public override string ToString()
        {
            return fileName;
        }
        
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
        public static File FromJson(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            
            return JsonUtility.FromJson<File>(value);;
        }
        
        public static File Empty {
            get { return new File("", ""); }
        }
    }
}