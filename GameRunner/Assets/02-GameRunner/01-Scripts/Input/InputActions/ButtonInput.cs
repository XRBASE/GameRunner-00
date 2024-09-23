using UnityEngine;

namespace Cohort.GameRunner.Input.Actions
{
    using Input = UnityEngine.Input;

    public class ButtonInput
    {
        private KeyCode[] _keyCodes;

        public ButtonInput(KeyCode[] keyCodes)
        {
            _keyCodes = keyCodes;
        }
        
        /// <param name="btnState">button is currently pressed</param>
        /// <returns>True/False has button value changed this update</returns>
        public bool ReadButtonVal(out bool btnState)
        {
            btnState = false;
            for (int i = 0; i < _keyCodes.Length; i++)
            {
                if (Input.GetKeyDown(_keyCodes[i]))
                {
                    btnState = true;
                    return true;
                }
                if (Input.GetKeyUp(_keyCodes[i]))
                {
                    btnState = false;
                    return true;
                }
            }
            return false;
        }
    }
}