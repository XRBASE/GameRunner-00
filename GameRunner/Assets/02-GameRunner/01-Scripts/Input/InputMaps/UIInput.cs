using System;
using UnityEngine.InputSystem;

namespace Cohort.Input.Maps
{
    /// <summary>
    /// Container for UI input actions.
    /// </summary>
    public class UIInput : InputMap
    {
        //called when switching to the next selectable element is required.
        public Action onSwitchElement;
        

        public UIInput(InputActionAsset action) : base(action, "UI")
        {
            //sub callback to the canceled (on button up) action
            _map["SwitchSelectedElement"].performed += ReadClick;
        }

        ~UIInput()
        {
            Dispose();
        }

        public override void Dispose()
        {
            _map["SwitchSelectedElement"].performed -= ReadClick;
            _map["SwitchSelectedElement"].Dispose();
            onSwitchElement = null;
            
            base.Dispose();
        }

        private void ReadClick(InputAction.CallbackContext obj)
        {
            onSwitchElement?.Invoke();
        }
    }
}