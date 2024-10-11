using Cohort.UI.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjIndicatorUI : UIPanel {
    [SerializeField] private RectTransform _parent;
    [SerializeField] private Image _indicatorPrefab;
    [SerializeField] private Sprite fallbackIco;

    private int _activeCount = 0;

    private void Awake() {
        UILocator.Register(this);
    }

    private void Destroy() {
        UILocator.Remove<ObjIndicatorUI>();
    }

    public Image CreateIndicator(Vector2 size, Color col, Sprite ico = null, string name = "unnamed_indicator") {
        Image indicator = Instantiate(_indicatorPrefab, _parent);
        if (ico == null) {
            ico = fallbackIco;
        }

        indicator.sprite = ico;
        indicator.name = name;
        indicator.color = col;
        ((RectTransform)indicator.transform).sizeDelta = size;
        
        return indicator;
    }

    public void OnActivate() {
        _activeCount++;
        
        if (_activeCount > 0) {
            Activate();
        }
    }
    
    public void OnDeactivate() {
        _activeCount = Mathf.Max(0, _activeCount--);

        if (_activeCount == 0) {
            Deactivate();
        }
    }

    public override void Activate() {
        //all ui should go over this one
        transform.SetAsFirstSibling();
        
        base.Activate();
    }
}
