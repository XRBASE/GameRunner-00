using System;
using Cohort.Ravel.BackendData.Data;
using Cohort.Ravel.BackendData.Images;
using UnityEngine;

namespace Cohort.Ravel.Assets
{
    /// <summary>
    /// Container class for images.
    /// </summary>
    [Serializable]
    public class Image : Asset
    {
        public Action<Sprite> onImageUpdated;
        
        private Sprite _image;
        private ImageSize _size;
        
        public Image(string id, string name) : base(id, name) { }
        
        /// <summary>
        /// Set the image contained in the class.
        /// </summary>
        /// <param name="image">sprite image to set.</param>
        /// <param name="size">size of the image that has been set.</param>
        public void SetImage(Sprite image, ImageSize size) {
            _image = image;
            _size = size;
        }
        
        /// <summary>
        /// Tries to retrieved cached image and otherwise starts the download.
        /// </summary>
        /// <param name="size">size that you want to download.</param>
        /// <param name="image">out variable for image, in the case it has been found.</param>
        /// <returns>True/False any image was found without async download call.</returns>
        public bool TryGetImage(ImageSize size, out Sprite image) {
            if (_size >= size) {
                image = _image;
                return true;
            }
            
            DataServices.Images.GetPreviewImage(id, size, OnImageUpdated);
            image = null;
            return false;
        }

        /// <summary>
        /// Called by webrequest when image has been retrieved.
        /// </summary>
        /// <param name="image">image that has been downloaded.</param>
        /// <param name="size">size of downloaded image.</param>
        private void OnImageUpdated(Sprite image, ImageSize size) {
            _size = size;
            _image = image;
            
            onImageUpdated?.Invoke(_image);
        }
        
        public override bool Overwrite(DataContainer data) {
            if (data is Image) {
                Image im = (Image)data;
                bool hasChanged = base.Overwrite(data);

                if (im._image != null) {
                    _image = im._image;
                }
                return hasChanged;
            }
			
            throw GetOverwriteFailedException(data);
        }
    }
}