using UnityEngine;

public class ImagePreview : MatchPreview
{
    public Texture2D texture2D;
    public override void SetPreview()
    {
        image.material.mainTexture = texture2D;
    }
}
