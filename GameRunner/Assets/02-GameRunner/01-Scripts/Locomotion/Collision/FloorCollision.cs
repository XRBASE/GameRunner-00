using System;
using MathBuddy.Flags;
using UnityEngine;

public class FloorCollision : MonoBehaviour
{
    private const int PLAYER_LAYER = 3;
    public Action<bool> onFloorCollision;
    private LayerMask _layerMask;
    private CapsuleCollider _capsuleCollider;
    private bool _hasContact;

    public void Initialise(LayerMask mask, Vector3 center, float radius, float height)
    {
        gameObject.layer = PLAYER_LAYER;
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
            if (!_hasContact)
                onFloorCollision?.Invoke(true);
            _hasContact = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_layerMask.MaskIncludes(other.gameObject.layer))
        {
            if (_hasContact)
            {
                onFloorCollision?.Invoke(false);
            }
            _hasContact = false;
        }
    }
}