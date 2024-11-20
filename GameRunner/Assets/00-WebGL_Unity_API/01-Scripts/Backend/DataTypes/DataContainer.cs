using System;

namespace Cohort.BackendData
{
    /// <summary>
    /// This class ensures that all forms of backend data can always be overwritten and updated with new backend data, whilst
    /// retaining any previously cached data
    /// </summary>
    public abstract class DataContainer
    {
        /// <summary>
        /// The key used to cache this item internaly (has to be unique per type per item)
        /// </summary>
        public abstract string Key { get; }

        protected Exception GetOverwriteFailedException(DataContainer data) {
            return new Exception($"Data from which {this.GetType()}: {Key} is being overwritten is not this type (Tying to overwrite with {data.GetType()}).");
        }

        /// <summary>
        /// Overwrite all fields of this item, with fields contained in data, but keep any fields that are filled in in this
        /// instance, but not in data parameter.
        /// </summary>
        /// <returns>True false overwritten result contains changes.</returns>>
        public abstract bool Overwrite(DataContainer data);

#region Operators
        public static bool operator ==(DataContainer lhs, DataContainer rhs)
        {
            //null means the same
            if (lhs is null || rhs is null) {
                return lhs is null && rhs is null;
            }

            //if types don't match, then they are not the same.
            if (lhs.GetType() != rhs.GetType()) {
                return false;
            }
            
            return lhs.Key == rhs.Key;
        }
        public static bool operator !=(DataContainer lhs, DataContainer rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object o)
        {
            if (o.GetType() == typeof(DataContainer)) {
                return (DataContainer) o == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

#endregion
        
    }
}