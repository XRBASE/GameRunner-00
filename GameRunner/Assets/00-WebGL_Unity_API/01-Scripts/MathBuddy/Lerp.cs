using UnityEngine;

namespace MathBuddy.TLerp {
    public static class Lerp {
        /// <summary>
        /// Lerps value A towards B scaled by time
        /// </summary>
        /// <param name="a">Start position of lerp</param>
        /// <param name="b">End position of lerp</param>
        /// <param name="t">delta time</param>
        /// <param name="r">constant factor te determine strength.</param>
        /// <returns>Time scaled interpolation between a and b.</returns>
        public static float TLerp(float a, float b, float t, float r) {
            return (a - b) * Mathf.Pow(r, t) + b;
        }

        /// <summary>
        /// Lerps value 0 towards 1 scaled by time
        /// </summary>
        /// <param name="t">delta time</param>
        /// <param name="r">constant factor te determine strength.</param>
        /// <returns>Time scaled interpolation between a and b.</returns>
        public static float TLerp01(float t, float r) {
            return -Mathf.Pow(r, t) + 1;
        }
    }
}