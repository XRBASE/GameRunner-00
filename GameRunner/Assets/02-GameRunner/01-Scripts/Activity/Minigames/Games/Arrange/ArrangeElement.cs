using UnityEngine;

public class ArrangeElement : MonoBehaviour
{
    public string id;
    public Sprite sprite;

    public void Initialise(ArrangeData data)
    {
        id = data.UID;
        sprite = data.sprite;
    }
}