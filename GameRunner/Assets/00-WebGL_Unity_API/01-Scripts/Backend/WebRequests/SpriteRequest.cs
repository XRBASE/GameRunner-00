using System.Collections.Generic;
using Cohort.Ravel.Networking;

public class SpriteRequest : RavelWebRequest
{
    /// <summary>
    /// Super simple constructor, just uses base version, does not add base url, full url needs to be provided.
    /// </summary>
    public SpriteRequest(string url) : base(url, Method.GetSprite)
    {
        _header = new Dictionary<string, string>();
        _header.Add("Content-Type", "application/json");
    }
}
