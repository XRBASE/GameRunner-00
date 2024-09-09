using System;
using System.Collections.Generic;
using UnityEngine;

namespace MathBuddy.Transforms
{
    public static class TransformExtensions
    {
        /// <summary>
        /// Apply data from this instance, to another transform.
        /// </summary>
        /// <param name="t">Transform to apply data to.</param>
        /// <param name="values">Values to apply.</param>
        public static void ApplyTransformValues(this Transform t, TransformValue values, TransformType applyTo = ~(TransformType.None))
        {
            if ((applyTo & TransformType.Position) > 0) {
                t.position = values.position;    
            }

            if ((applyTo & TransformType.Rotation) > 0) {
                t.rotation = values.rotation;
            }

            if ((applyTo & TransformType.Scale) > 0) {
                t.localScale = values.scale;
            }
        }
        
        /// <summary>
        /// Check if transform a is directed at transform b
        /// </summary>
        public static bool TransformsFacing(Transform a, Transform b)
        {
            //flat vector for cam forward
            Vector3 c = a.position - b.position;
            c.y = 0f;
            c.Normalize();
            //flat vector for icon forward
            //use parent as parent is centered on the middle of the badge
            Vector3 i = a.forward;
            i.y = 0f;
            i.Normalize();

            float angle = Mathf.Rad2Deg * Mathf.Acos(c.x * i.x + c.z * i.z);
        
            //We check if angle is smaller, because the z of the cam is supposed to be directed towards the z of the icons 
            return (float.IsNaN(angle) || Mathf.Abs(angle) <= 90);
        }
        
        /// <summary>
        /// Finds nested children by name in a Transform
        /// </summary>
        /// <param name="aParent">parent transform</param>
        /// <param name="aName">the name of the transform you want to find</param>
        /// <returns>Nested child or null</returns>
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0)
            {
                var c = queue.Dequeue();
                if (c.name == aName)
                    return c;
                foreach(Transform t in c)
                    queue.Enqueue(t);
            }
            return null;
        }    
        
        /// <summary>
        /// Finds the child of a transform but starts searching from the deepest child
        /// </summary>
        public static Transform FindDeepChildDepthFirst(this Transform aParent, string aName)
        {
            foreach(Transform child in aParent) {
                if (child.name == aName)
                    return child;
                var result = child.FindDeepChild(aName);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        public static void SetLayer(this Transform obj, int layer) {
            obj.gameObject.layer = layer;
            
            for (int i = 0; i < obj.childCount; i++) {
                obj.GetChild(i).SetLayer(layer);
            }
        }
    }
    
    /// <summary>
    /// Used to save and serialize values of a transform and (re)apply them to a transform
    /// </summary>
    public struct TransformValue
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        /// <summary>
        /// Create instance save of a transform, so you can apply it to another transform.
        /// </summary>
        /// <param name="t">transform to save data of</param>
        public TransformValue(Transform t)
        {
            this.position = t.position;
            this.rotation = t.rotation;
            this.scale = t.localScale;
        }
    }
    
    [Flags]
    public enum TransformType
    {
        None = 0,
        Position = 1,
        Rotation = 2,
        Scale = 4,
    }
}