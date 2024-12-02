using System;
using MathBuddy.Flags;
using UnityEngine;

public class FloorCollision : MonoBehaviour
{
    public Action<bool> onFloorCollision;
    private LayerMask _layerMask;
    private CapsuleCollider _capsuleCollider;
    private int _contacts;


    public void Initialise(LayerMask mask, Vector3 center, float radius, float height)
    {
        _layerMask = mask;
        _capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
        _capsuleCollider.radius = radius;
        _capsuleCollider.center = center;
        _capsuleCollider.height = height;
        _capsuleCollider.isTrigger = true;
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