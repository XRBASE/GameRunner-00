using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSliderUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI sliderTitle;
    public TextMeshProUGUI sliderPercentage;

    public void SetSlider(float percentage)
    {
        slider.value = percentage;
        percentage = percentage * 100;
        sliderPercentage.text = $"{percentage:F0}%";
    }
}
