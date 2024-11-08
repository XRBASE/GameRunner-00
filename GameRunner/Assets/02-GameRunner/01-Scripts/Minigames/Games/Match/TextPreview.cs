public class TextPreview : MatchPreview
{
    public string text;
    public override void SetPreview()
    {
        textPreview.text = text;
    }
}
