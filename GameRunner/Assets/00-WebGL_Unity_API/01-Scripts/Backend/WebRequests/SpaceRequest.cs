namespace Cohort.Ravel.Networking.Spaces
{
    /// <summary>
    /// Overlaying class to retrieve specific web requests from the space subset of the backend.
    /// </summary>
    public class SpaceRequest : TokenWebRequest {
        /// <summary>
        /// Base constructor for space webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="method">method of webcall (Post, Get, Put, etc)</param>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/connections/</param>
        /// <param name="version">version modifier to add to the called url, v1/ by default.</param>
        private SpaceRequest(Method method, string postfix, string prefix = "space", string data = "") : base(
            method, "api/") {
            if (string.IsNullOrEmpty(postfix)) {
                _url += prefix;
            }
            else if (string.IsNullOrEmpty(prefix)) {
                _url += $"{postfix}";
            }
            else {

                _url += $"{prefix}/{postfix}";
            }

            if (!string.IsNullOrEmpty(data)) {
                _data = data;
            }
        }

        /// <summary>
        /// Retrieves space based on its uuid.
        /// </summary>
        /// <param name="uuid">uuid of space.</param>
        public static SpaceRequest GetSpace(string uuid) {
            return new SpaceRequest(Method.Get, $"{uuid}");
        }

        /// <summary>
        /// This call changes one of mutliple entries of the dynamic conent of a space. All names, mentioned in the json,
        /// will be overrwritten with the values provided in the json file.
        /// </summary>
        /// <param name="spaceUuid">uuid of the space of which the content is being set.</param>
        /// <param name="contentJson">json string, containing the updated content for the space
        /// (only entries that change, other entries are preserved.)</param>
        public static SpaceRequest PublishSpaceSlot(string spaceUuid, string slotName, string assetId) {
            string json = "{\"slotName\": \"" + slotName +
                          "\",\"assetId\": \"" + assetId +
                          "\"}";

            return new SpaceRequest(Method.PatchJSON, $"{spaceUuid}/slot", "space", json);
        }

        /// <summary>
        /// Retrieves environment based on its uuid.
        /// </summary>
        /// <param name="uuid">uuid of environment.</param>
        public static SpaceRequest GetEnvironment(string uuid) {
            return new SpaceRequest(Method.Get, $"environment/{uuid}", "");
        }

        /// <summary>
        /// Get all spaces that have been added to an organisation.
        /// </summary>
        /// <param name="organisationId">id of the organisation of which the spaces will be retrieved.</param>
        public static SpaceRequest GetSpaces() {
            return new SpaceRequest(Method.Get, $"", "spaces");
        }
    }
}