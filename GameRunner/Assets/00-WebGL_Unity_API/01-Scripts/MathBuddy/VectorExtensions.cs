using UnityEngine;

namespace MathBuddy.Vectors
{
    /// <summary>
    /// Extension class for vector help methods.
    /// </summary>
    public static class VectorExtensions
    {
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        /// <summary>
        /// Rotates coordinate of vector v by angle degrees (CW).
        /// </summary>
        /// <param name="v">Vector coordinate to rotate.</param>
        /// <param name="angle">Angle with which to rotate the vector.</param>
        /// <returns></returns>
        public static Vector2 Rotate(Vector2 v, float angle)
        {
            angle *= Mathf.Deg2Rad;
            Vector2 n;
            n.x = Mathf.Cos(angle) * v.x - Mathf.Sin(angle) * v.y;
            n.y = Mathf.Sin(angle) * v.x + Mathf.Cos(angle) * v.y;

            return n;
        }

        /// <summary>
        /// Returns vector, scaled by the aspect of the resolution using the type scaling method.
        /// </summary>
        /// <param name="origin">Vector that will be scaled.</param>
        /// <param name="type">Type of method with which to scale the vector.</param>
        /// <param name="resolution">Resolution from which the aspect will be defined (result is not clamped in resolution, but will match it's the aspect)</param>
        public static Vector2 GetScaledVectorByAspect(this Vector2 origin, AspectScalingType type, Vector2 resolution)
        {
            float aspect;
            Vector2 result;
            switch (type)
            {
                case AspectScalingType.ScaleWidth:
                    //scales x by the aspect of x units per y unit
                    aspect = resolution.x / resolution.y;
                    result = origin;
                    result.x *= aspect;
                    return result;
                case AspectScalingType.ScaleHeight:
                    //scales y by the aspect of y units per x unit
                    aspect = resolution.y / resolution.x;
                    result = origin;
                    result.y *= aspect;
                    return result;
                case AspectScalingType.ScaleMax:
                    //determine biggest size and scale that axis back to match resolution.
                    if (resolution.x < resolution.y)
                    {
                        return origin.GetScaledVectorByAspect(AspectScalingType.ScaleWidth, resolution);
                    }
                    else
                    {
                        return origin.GetScaledVectorByAspect(AspectScalingType.ScaleHeight, resolution);
                    }
                default:
                    return origin;
            }
        }
    }


    public enum AspectScalingType
    {
        None = 0,

        //scale x axis
        ScaleWidth = 1,

        //scale y axis
        ScaleHeight = 2,

        //scale largest axis
        ScaleMax = 3,
    }
}