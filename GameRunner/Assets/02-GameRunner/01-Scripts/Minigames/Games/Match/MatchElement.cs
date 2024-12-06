using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchElement : MonoBehaviour
{
    public int id;
    public MatchType matchType;
    public TextMeshProUGUI title;
    public Button button;
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

    public void Initialise()
    {
        
    }

}
