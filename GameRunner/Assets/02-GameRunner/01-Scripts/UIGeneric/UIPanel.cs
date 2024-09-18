using UnityEngine;

namespace Cohort.UI.Generic {
    /// <summary>
    /// Not required, but usefull base class that contains some basic UI panel functionalities.
    /// </summary>
    public class UIPanel : MonoBehaviour {
        public RectTransform RectTransform {
            get { return (RectTransform)transform; }
        }

        public virtual void Activate() {
            gameObject.SetActive(true);
        }

        public virtual void Deactivate() {
            gameObject.SetActive(false);
        }
    }
}