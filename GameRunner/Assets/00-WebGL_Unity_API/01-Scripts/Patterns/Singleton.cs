using System;
using UnityEngine;

namespace Cohort.Patterns
{
    [DefaultExecutionOrder(-1)]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (!Instance)
                Instance = this as T;
            else
                throw new Exception($"Double singleton exception of type {typeof(T)} ({gameObject.name})!");
        }
    }
}