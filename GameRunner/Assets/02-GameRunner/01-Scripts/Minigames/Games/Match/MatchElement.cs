using TMPro;
using UnityEngine;

public class MatchElement : MonoBehaviour
{
    public MatchType matchType;
    private MatchPreview _matchPreview;
    
    public enum MatchType{Image, Text}
    
    public void SetPreviewElement(MatchPreview matchPreview)
    {
        switch (matchPreview)
        {
            case ImagePreview:
                matchType = MatchType.Image;
                break;
            case TextPreview:
                matchType = MatchType.Text;
                break;
        }
        _matchPreview = matchPreview;
    }

}
