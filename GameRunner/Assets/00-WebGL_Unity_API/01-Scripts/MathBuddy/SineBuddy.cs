using UnityEngine;
using UnityEngine.Events;

public class SineBuddy : MonoBehaviour
{
    public UnityEvent<float> onSine = new UnityEvent<float>();
    
    [SerializeField] private float _magnitude = 1;
    [SerializeField] private float _amplitude = 1;
    [SerializeField] private float _range = 1;
    [SerializeField] private float _sin;
    private float _counter;

    private void Awake()
    {
        _counter = 0f;
    }
    
    void Update()
    {
        _counter += Time.deltaTime / _amplitude;
        if (_counter >= _range) {
            _counter = 0f;
        }
        _sin = Mathf.Sin(_counter) * _magnitude;
        onSine?.Invoke(_sin);
    }
}
