using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;

    public void SetHealth(float currHp, float maxHP)
    {
        healthSlider.maxValue = maxHP;
        healthSlider.value = currHp;
    }
}
