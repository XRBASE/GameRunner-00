using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
        public static InputManager Instance
        {
                get
                {
                        if (_inputManager == null)
                        {
                                _inputManager = new GameObject("InputManager").AddComponent<InputManager>();
                        }
                        return _inputManager;
                }
                set { _inputManager = value; }
        }

        private static InputManager _inputManager;

        public InputActionAsset inputActionAsset;
        private InputActionMap _map;
        
        private void Awake()
        {
                inputActionAsset = Resources.Load<InputActionAsset>("TestCameraInputActions");
                _map = inputActionAsset.FindActionMap("Player");
                DontDestroyOnLoad(this);
        }
        public InputAction GetInputAction(string name)
        {
                return _map.FindAction(name);
        }
}
