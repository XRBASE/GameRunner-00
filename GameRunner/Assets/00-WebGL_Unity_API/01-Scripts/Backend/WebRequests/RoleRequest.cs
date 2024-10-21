namespace Cohort.Networking.Roles
{
    /// <summary>
    /// Overlaying class to retrieve specific web requests from the role subset of the backend.
    /// </summary>
    public class RoleRequest : TokenWebRequest
    {
        /// <summary>
        /// Base constructor for role webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="method">method of webcall (Post, Get, Put, etc)</param>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/appauth/</param>
        /// <param name="version">version modifier to add to the called url, v1/ by default.</param>
        public RoleRequest(Method method, string postfix) : base(method, "api/")
        {
            _url += "appauth/" + postfix;
        }

        /// <summary>
        /// Retrieves list of role responses including matching permission set.
        /// </summary>
        public static RoleRequest GetAllRoles()
        {
            return new RoleRequest(Method.Get, "roles/permissions");
        }
    }
}