using System;
using System.Collections.Generic;

namespace Cohort.UI.Generic {
    /// <summary>
    /// Service locator for registering and retrieving difference UI panels.
    /// </summary>
    public sealed class UILocator {
        private static readonly Dictionary<Type, object> _panels = new Dictionary<Type, object>();

        public static void Register<T>(T panel) {
            _panels[typeof(T)] = panel;
        }

        public static void Remove<T>() {
            _panels.Remove(typeof(T));
        }

        public static T Get<T>() {
            return (T)_panels[typeof(T)];
        }
    }
}