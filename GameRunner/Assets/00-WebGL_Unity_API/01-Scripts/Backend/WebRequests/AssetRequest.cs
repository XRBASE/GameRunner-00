using System.IO;
using Cohort.Ravel.BackendData.Images;
using UnityEngine;

namespace Cohort.Ravel.Networking.Spaces
{
    public class AssetRequest : TokenWebRequest
    {
        private const string PREFIX = "assets";
        
        /// <summary>
        /// Base constructor for space webrequests. api/ is also added, as this is not a constant part of the webrequests.
        /// </summary>
        /// <param name="method">method of webcall (Post, Get, Put, etc)</param>
        /// <param name="postfix">Last part of the webrequest: part after: BaseUrl/connections/</param>
        /// <param name="version">version modifier to add to the called url, v1/ by default.</param>
        private AssetRequest(Method method, string postfix, string api = "") : base(method, api)
        {
            if (string.IsNullOrEmpty(postfix)) {
                _url += PREFIX;
            }
            else {
                _url += $"{PREFIX}/{postfix}";
            }
        }
        
        private AssetRequest(string postfix, WWWForm form) : base("api/", form) {
            if (string.IsNullOrEmpty(postfix)) {
                _url += PREFIX;
            }
            else {
                _url += $"{PREFIX}/{postfix}";
            }
        }
        
        /// <summary>
        /// Uploads the raw data to an asset.
        /// </summary>
        /// <param name="uuid">id of the asset.</param>
        /// <param name="assetPath">path at which the data has been saved.</param>
        public static AssetRequest UploadAsset(string uuid, string assetPath) {
            WWWForm form = new WWWForm();
            string fName = Path.GetFileName(assetPath);
            form.AddBinaryData("image", File.ReadAllBytes(assetPath), fName);

            return new AssetRequest($"{uuid}/Upload", form);
        }
        
        /// <summary>
        /// Downloads raw data of an asset.
        /// </summary>
        /// <param name="guid">uuid of the asset.</param>
        public static AssetRequest DownloadAsset(string guid) {
            return new AssetRequest(Method.Get, guid);
        }

        public static string GetDownloadURL(string uuid) {
            return new AssetRequest(Method.Get, uuid)._url;
        }
        
        /// <summary>
        /// Downloads an assetbundle.
        /// </summary>
        /// <param name="uuid">uuid of the asset.</param>
        public static AssetRequest DownloadAssetBundle(string uuid) {
            return new AssetRequest(Method.GetAssetBundle, uuid);
        }
        
        /// <summary>
        /// Downloads an unscaled version of an image asset.
        /// </summary>
        /// <param name="uuid">uuid of the asset.</param>
        public static AssetRequest DownloadPreviewImageAsset(string uuid) {
            return new AssetRequest(Method.GetSprite, uuid);
        }
        
        /// <summary>
        /// Downloads a scaled version of the image.
        /// </summary>
        /// <param name="uuid">uuid of the asset.</param>
        /// <param name="size">image size for the asset.</param>
        /// <param name="aspect">aspect ratio (backend not yet functional).</param>
        public static AssetRequest DownloadScaledPreviewImageAsset(string uuid, ImageSize size, ImageAspect aspect) {
            ImageService.GetDimensions(size, aspect, out int x, out int y);
            
            return new AssetRequest(Method.GetSprite, $"{uuid}/scale/{x}x{y}");
        }

        public static AssetRequest GetPublicAssets() {
            return new AssetRequest(Method.Get, "", "api/");
        }
    }
}