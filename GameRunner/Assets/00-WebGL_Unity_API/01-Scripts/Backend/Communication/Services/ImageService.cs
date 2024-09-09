using System;
using Cohort.Ravel.Networking;
using Cohort.Ravel.Networking.Spaces;
using UnityEngine;

namespace Cohort.Ravel.BackendData.Images
{
    /// <summary>
    /// For not not connected to backend, download handler for images. Downloads images and stores them using given key
    /// </summary>
    public class ImageService : MonoBehaviour
    {
        /// <summary>
        /// Starts and returns the image download routine, for given url.
        /// </summary>
        /// <param name="uuid">asset UUID</param>
        /// <param name="callback">callback called with downloaded image.</param>
        public void GetPreviewImage(string uuid, ImageSize size, Action<Sprite, ImageSize> callback)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return;
            
            //PHOE: we can implement the scaling here and also implement the aspect, but this hasn't been done yet.
            AssetRequest req = AssetRequest.DownloadScaledPreviewImageAsset(uuid, size, ImageAspect.A16x9);
            AsyncOperation ao = req.Send();
            
            ao.completed += (operation) => OnSpriteRetrieved(req, size, callback);
        }

        private void OnSpriteRetrieved(AssetRequest req, ImageSize size, Action<Sprite, ImageSize> callback)
        {
            RavelWebResponse res = new RavelWebResponse(req);
            if (res.Success && res.TryGetSprite(out Sprite result)) {
                callback?.Invoke(result, size);
            }
            else {
                Debug.LogError($"Error downloading sprite: {res.Error.FullMessage}.");
            }
        }
        
        /// <summary>
        /// Get dimension values based on size and aspect.
        /// </summary>
        /// <param name="s">size value</param>
        /// <param name="a">aspect value</param>
        /// <param name="x">width in pixels</param>
        /// <param name="y">height in pixels</param>
        public static void GetDimensions(ImageSize s, ImageAspect a, out int x, out int y) {
            switch (s) {
                case ImageSize.I256:
                    x = 256;
                    break;
                default:
                case ImageSize.I512:
                    x = 512;
                    break;
                case ImageSize.I1024:
                    x = 1024;
                    break;
                case ImageSize.I1280:
                    x = 1280;
                    break;
                case ImageSize.I1920:
                    x = 1920;
                    break;
            }

            switch (a) {
                case ImageAspect.A1x1:
                    y = x;
                    break;
                case ImageAspect.A16x9:
                    y = Mathf.RoundToInt(x * 0.5625f);
                    break;
                default: {
                    Debug.LogError($"Scaling for image aspect {a} not implemented!");
                    y = 0;
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Size enums
    /// </summary>
    public enum ImageSize
    {
        None = 0,
        I256 = 1,
        I512 = 2,
        I1024 = 3,
        I1280 = 4,
        I1920 = 5,
    }
    
    /// <summary>
    /// Aspect enums
    /// </summary>
    public enum ImageAspect
    {
        None = 0,
        A1x1 = 1,
        A16x9 = 2
    }
    
    /// <summary>
    /// Struct to save the different sizing urls in for images.
    /// </summary>
    [Serializable]
    public struct ImageSizeUrls
    {
        public string url256,
                       url512,
                       url1024,
                       url1280,
                       url1920;

        public bool TryGetUrl(ImageSize size, out string url)
        {
            switch (size) {
                case ImageSize.I256:
                    url = url256;
                    break;
                case ImageSize.I512:
                    url = url512;
                    break;
                case ImageSize.I1024:
                    url = url1024;
                    break;
                case ImageSize.I1280:
                    url = url1280;
                    break;
                case ImageSize.I1920:
                    url = url1920;
                    break;
                default:
                    url = "";
                    return false;
            }

            return true;
        }
    }
}