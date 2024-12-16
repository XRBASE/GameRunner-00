using System;
using System.Collections.Generic;
using MathBuddy.Flags;
using UnityEngine;

public class FloorCollision : MonoBehaviour
{
    private const int PLAYER_LAYER = 3;
    public Action<bool> onFloorCollision;
    private LayerMask _layerMask;
    private CapsuleCollider _capsuleCollider;
    private readonly List<Collider> _contacts = new List<Collider>();

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
            _contacts.Add(other);
            RemoveDestroyed();
            if (_contacts.Count == 1)
            {
                onFloorCollision?.Invoke(true);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_layerMask.MaskIncludes(other.gameObject.layer))
        {
            _contacts.Remove(other);
            RemoveDestroyed();
            if (_contacts.Count == 0)
            {
                onFloorCollision?.Invoke(false);
            }
        }
    }

    private void RemoveDestroyed()
    {
        if (_contacts != null)
        {
            for (int i = 0; i < _contacts.Count; i++)
            {
                if (_contacts[i] == null)
                {
                    _contacts.Remove(_contacts[i]);
                    i--;
                }
            }
        }
    }
}