using Cohort.BackendData;
using Cohort.Assets;

using System;

namespace Cohort.Spaces
{
    //for now not with custom repo, but might be needed later
    [Serializable]
    public class Environment : DataContainer
    {
        /// <summary>
        /// Returns Unity scene name
        /// </summary>
        public override string Key {
            get { return id; }
        }

        public string id;
        public string name;
        
        public Image previewImage;
        
        /// <summary>
        /// Add all data in the given class in this class, without clearing the data of this class.
        /// </summary>
        /// <param name="data">data to overwrite</param>
        public override bool Overwrite(DataContainer data)
        {
            if (data.GetType() == typeof(Environment)) {
                Environment e = (Environment)data;
                bool hasChanges = false;
                
                if (!string.IsNullOrEmpty(e.name))
                {
                    name = e.name;
                    hasChanges = true;
                }

                return hasChanges;
            }

            throw GetOverwriteFailedException(data);
        }
    }
}