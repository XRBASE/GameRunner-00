using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WordGameInput : MonoBehaviour
{
    private readonly string[] _alphabet =
    {
        "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V",
        "W", "X", "Y", "Z"
    };

    private const string _actionMapName = "WordGame";
    private InputActionMap _wordGameActionMap;
    public InputActionAsset inputActionAsset;
    public Action<string> onKeyboardInput;
    public Action submit;
    public Action remove;

    private void Awake()
    {
        
        _wordGameActionMap = inputActionAsset.FindActionMap(_actionMapName);
        for (int i = 0; i < _alphabet.Length; i++)
        {
            _wordGameActionMap[_alphabet[i]].performed += context => {onKeyboardInput?.Invoke(context.control.name);};
        }
        _wordGameActionMap["Remove"].performed += _ => remove?.Invoke();
        _wordGameActionMap["Submit"].performed += _ => submit?.Invoke();
    }
}
