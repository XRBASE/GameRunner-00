using UnityEngine;

namespace MathBuddy.Lines
{
    public static class LinesExtensions
    {
        /// <summary>
        /// Find the closest point to P on line AB 
        /// </summary>
        /// <param name="a">first point on line.</param>
        /// <param name="b">second point on line.</param>
        /// <param name="p">point of which to find the closest point on the line.</param>
        public static Vector3 GetClosestPointOnLine(Vector3 a, Vector3 b, Vector3 p)
        {
            Vector3 a_to_p = p - a; // line from a to p
            Vector3 a_to_b = b - a; // line from a to b

            //dot product a-p and a-b
            float dot = Vector3.Dot(a_to_p, a_to_b);
            //normalized distance of closest point to p on line AB 
            float t = Mathf.Clamp01(dot / a_to_b.sqrMagnitude);
            
            //add product of normalized closest point and line AB to A
            return a + a_to_b * t;
        }
    }
}
