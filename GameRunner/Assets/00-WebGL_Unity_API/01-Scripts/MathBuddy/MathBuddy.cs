//generic overcoupling mathbuddy class for commonly used features

using System;
using UnityEngine;

namespace MathBuddy
{
    public static class FloatingPoints
    {
        //0.0000000001f in scientific notation
        public const float LABDA = 1e-6f;
        
        public static bool Equals(float a, float b)
        {
            return Mathf.Abs(a - b) <= LABDA;
        }
        
        public static bool IsInBounds(float value, float min, float max)
        {
            return value < max && value > min;
        }
    }

    namespace FloatExtentions
    {
        [Serializable]
        public class Range
        {
            /// <summary>
            /// Total span between min and max.
            /// </summary>
            public float Span {
                get { return max - min; }
            }
            
            public float A {
                get { return (_isPositive) ? min : max; }
            }
            
            public float B {
                get { return (_isPositive) ? max : min; }
            }
            public float min, max;
            
            
            [SerializeField, Tooltip("Direction of range, determines where t=0 is.")] 
            private bool _isPositive = true;

            public Range(float b)
            {
                if (b > 0f) {
                    _isPositive = true;
                    min = 0f;
                    max = b;
                }
                else {
                    _isPositive = false;
                    max = 0f;
                    min = b;
                }
            }
            
            public Range(float a, float b)
            {
                if (b > a) {
                    _isPositive = true;
                    min = a;
                    max = b;
                }
                else {
                    _isPositive = false;
                    max = a;
                    min = b;
                }
            }

            /// <summary>
            /// Retrieves value at given time.
            /// </summary>
            public float GetValue(float t, bool clamp = false)
            {
                if (clamp) {
                    t = Mathf.Clamp01(t);
                }
                return A + (B - A) * t;
            }

            /// <summary>
            /// Retrieves time at given value.
            /// </summary>
            public float GetTime(float value, bool clamp = false)
            {
                if (clamp) {
                    value = Mathf.Clamp(value, min, max);
                }

                return (value - A) / (B - A);
            }

            public override string ToString()
            {
                return $"range({A}, {B})";
            }
        }
    }
}