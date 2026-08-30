using UnityEngine;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private HealthView healthView;

    private HealthPresenter healthPresenter;

    private void Start()
    {
        healthPresenter = new HealthPresenter(health, healthView);
    }

    private void OnDestroy()
    {
        healthPresenter?.Dispose();
    }
}