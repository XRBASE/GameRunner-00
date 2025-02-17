using UnityEngine;
using TMPro;

public class VersionNumberUI : MonoBehaviour {
    [SerializeField] private TMP_Text _versionField;

    private void Awake() {
        _versionField.text = Application.version;
    }
}
