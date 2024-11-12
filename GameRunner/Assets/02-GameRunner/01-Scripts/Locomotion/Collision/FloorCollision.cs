using System;
using MathBuddy.Flags;
using UnityEngine;

public class FloorCollision : MonoBehaviour
{
    public Action<bool> onFloorCollision;
    private LayerMask _layerMask;
    private BoxCollider _boxCollider;
    private int _contacts;

    public void Initialise(LayerMask mask)
    {
        _layerMask = mask;
        _boxCollider = gameObject.AddComponent<BoxCollider>();
        _boxCollider.size = new Vector3(.3f, 1f, .3f);
        _boxCollider.center = Vector3.down * .25f;
        _boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_layerMask.MaskIncludes(other.gameObject.layer))
        {
            _contacts++;
            if (_contacts == 1)
                onFloorCollision?.Invoke(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_layerMask.MaskIncludes(other.gameObject.layer))
        {
            _contacts--;
            if (_contacts == 0)
                onFloorCollision?.Invoke(false);
        }
    }
}