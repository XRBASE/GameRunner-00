using System;
using Cohort.Ravel.Assets;
using Cohort.Ravel.BackendData.Data;
using Cohort.Ravel.Permissions;

namespace Cohort.Ravel.Users
{
    [Serializable]
    public class User : DataContainer
    {
        public override string Key {
            get { return id; }
        }

        /// <summary>
        /// Firstname Lastname (of user).
        /// </summary>
        public string FullName {
            get { return $"{firstName} {lastName}"; }
        }

        //Ravel ID
        public string id;
        
        //get userdata response
        public string userName;
        public string firstName;
        public string lastName;
        public string rpmAvatarUri;

        public string[] domains;

        //disabled so it is not visible for unique users.
        public string email;
        
        //PHOE: profile image implementation
        public Image profileImage;

        public Role role;

        public User()
        {
            
        }
        
        public User(string uuid)
        {
            id = uuid;
        }
        
        /// <summary>
        /// Overwrite this user's data with other userdata. Any filled in field in other will be applied to this user.
        /// </summary>
        /// <param name="other">Other user to merge into this user.</param>
        public override bool Overwrite(DataContainer data)
        {
            if (data.GetType() == typeof(User)) {
                bool hasChanges = false;
                User other = (User) data;
                
                if(!string.IsNullOrEmpty(other.id)) {
                    id = other.id;
                    hasChanges = true;
                }
                if(!string.IsNullOrEmpty(other.firstName)) {
                    firstName = other.firstName;
                    hasChanges = true;
                }
                if(!string.IsNullOrEmpty(other.lastName)) {
                    lastName = other.lastName;
                    hasChanges = true;
                }
                if(!string.IsNullOrEmpty(other.email)) {
                    email = other.email;
                    hasChanges = true;
                }
                if (!string.IsNullOrEmpty(other.rpmAvatarUri)) {
                    rpmAvatarUri = other.rpmAvatarUri;
                    hasChanges = true;
                }
                if (other.profileImage != null) {
                    profileImage = other.profileImage;
                    hasChanges = true;
                }

                if (other.role != null) {
                    role = other.role;
                    hasChanges = true;
                }

                if (other.domains != null) {
                    domains = other.domains;
                    hasChanges = true;
                }
                
                return hasChanges;
            }

            throw GetOverwriteFailedException(data);
        }
        
        /// <summary>
        /// Return username(full) and userid. 
        /// </summary>
        public override string ToString() { return $"{FullName}({id})"; }
    }
}