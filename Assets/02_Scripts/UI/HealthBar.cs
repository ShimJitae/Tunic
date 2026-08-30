using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    private float currHPValue;

    public void SetMaxValue(float maxValue)
    {
        healthBar.maxValue = maxValue;
    }

    public void UpdateHealthBar(float value)
    {
        currHPValue += value;
        healthBar.value = currHPValue;
    }
}
