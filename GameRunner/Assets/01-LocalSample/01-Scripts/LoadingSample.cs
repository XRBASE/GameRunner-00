using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingSample : MonoBehaviour {
    [SerializeField] private GameObject _parent;
    [SerializeField] private Slider _loadingbar;
    [SerializeField] private TMP_Text _processStep;
    
    private float _loadDur = 5f;
    private string[] _steps = new string[] { "Making sacrifices to the RNG", "Writing some missing code", "Making coffee", "Smoke break", "Writing more code", "Another coffee break", "Uploading weird pictures to the internet" };

    private void Awake() {
        _parent.gameObject.SetActive(false);
    }

    public void StartLoading() {
        _parent.gameObject.SetActive(true);
        StartCoroutine(DoLoad());
    }

    private IEnumerator DoLoad() {
        float t = 0f;
        float dec = 0;
        
        int i = 0;
        int pI = -1;
        
        while (t < _loadDur) {
            t += Time.deltaTime;
            dec = t / _loadDur;
            i = Mathf.RoundToInt((dec) * (_steps.Length - 1));
            
            if (pI != i) {
                _processStep.text = _steps[i];
                pI = i;
            }

            _loadingbar.value = dec;
            SampleServerHandle.Instance.OnLoadingChanged(_steps[i], dec, false);
            yield return null;
        }
        
        SampleServerHandle.Instance.OnLoadingChanged("Finished", 1f, true);
        
        _parent.gameObject.SetActive(false);
    }
}
