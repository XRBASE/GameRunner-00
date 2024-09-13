using UnityEngine.InputSystem;

namespace Cohort.Input
{
    public abstract class InputMap
    {
        public string MapName
        {
            get { return _mapName; }
        }

        private string _mapName;

        public bool Disposed { get; private set; }

        public InputActionMap Map
        {
            get { return _map; }
        }

        protected InputActionMap _map;

        public InputMap(InputActionAsset actions, string mapName)
        {
            _mapName = mapName;
            _map = actions.FindActionMap(mapName);
        }

        /// <summary>
        /// Destructor called irregularly, it is better to manually call dispose.
        /// </summary>
        ~InputMap()
        {
            if (!Disposed)
            {
                Dispose();
            }
        }

        //don't forget to set Disposed on true
        /// <summary>
        /// Clear references and subscriptions.
        /// </summary>
        public virtual void Dispose()
        {
            _map.Dispose();
            Disposed = true;
        }
    }

    public enum ActionState
    {
        Started,
        Active,
        Canceled,
    }
}