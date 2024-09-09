using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MathBuddy.Flags
{
    /// <summary>
    /// Extentions class for enum flag behaviours.
    /// </summary>
    public static class FlagExtensions
    {
        /// <summary>
        /// Check whether the int of a layer is included in the flags that are contained within the layermask
        /// </summary>
        public static bool MaskIncludes(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        
        /// <summary>
        /// Use object and int cast to get int from enum
        /// </summary>
        /// <param name="e">Enum to convert.</param>
        /// <returns>Integer value of enum.</returns>
        public static int ToIntValue(this Enum e)
        {
            return (int)(object)e;
        }

        /// <summary>
        /// Use object and T cast to convert int back into enum
        /// (watch out for out of range scenario's).
        /// </summary>
        /// <param name="i">Integer representing enum value.</param>
        /// <typeparam name="T">Type of enum to cast back into.</typeparam>
        /// <returns>Enum value of integer.</returns>
        public static T ToEnumValue<T>(this int i) where T : Enum
        {
            return (T) (object) i;
        }
        
        /// <summary>
        /// Get IEnumerable collection of flags contained in one enum value.
        /// </summary>
        /// <param name="e">flag value of an enum</param>
        public static IEnumerable<T> GetFlagsNumerator<T>(Enum e) where T : Enum
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).Cast<T>();
        }

        public static List<string> GetFlagsEnumValues<T>(Enum e) where T : Enum {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag).Cast<string>().ToList();
        }
        
        public enum CheckType
        {
            All,
            Any
        }
        
        public enum FlagPositive
        {
            Contains,
            DoesNotContains
        }
    }
}