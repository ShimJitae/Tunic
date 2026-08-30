using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    public void SetHealth(float current, float max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = current;
    }
}
