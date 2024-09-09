using UnityEngine;

namespace MathBuddy.MonoBehaviours
{
	public static class MonobehaviourExtentions
	{
		public static T GetOrAddComponent<T>(this GameObject obj) where T : Behaviour {
			T item = obj.GetComponent<T>();
			
			if (item == null) {
				return obj.AddComponent<T>();
			}
			
			return item;
		}
	}
}
