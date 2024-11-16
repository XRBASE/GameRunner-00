using System;
using MathBuddy.Flags;
using UnityEngine;

public class FloorCollision : MonoBehaviour
{
    public Action<bool> onFloorCollision;
    private LayerMask _layerMask;
    private BoxCollider _boxCollider;
    private int _contacts;


    public void Initialise(LayerMask mask, Vector3 size, Vector3 center)
    {
        _layerMask = mask;
        _boxCollider = gameObject.AddComponent<BoxCollider>();
        _boxCollider.size = size;
        _boxCollider.center = center;
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