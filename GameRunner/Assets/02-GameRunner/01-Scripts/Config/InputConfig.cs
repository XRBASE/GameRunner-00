using UnityEngine;

namespace Cohort.Config {
	[CreateAssetMenu(menuName = "Cohort/Config/Input", fileName = "InputConfig", order = 0)]
	public class InputConfig : ScriptableObject {
		public const int IGNORE_RAYCAST_LAYER = 2;

		public static InputConfig Config {
			get {
				if (_instance == null) {
					_instance = Resources.Load<InputConfig>("Config/InputConfig");
				}

				return _instance;
			}
		}

		private static InputConfig _instance;

		public LayerMask interactionLayerMask;
	}
}