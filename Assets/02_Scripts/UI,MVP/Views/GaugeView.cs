using UnityEngine;
using UnityEngine.UI;

public class GaugeView : MonoBehaviour
{
    [SerializeField] protected Slider gaugeSlider;

    public void SetGauge(float currValue, float maxValue)
    {
        gaugeSlider.maxValue = maxValue;
        gaugeSlider.value = currValue;
    }
}
