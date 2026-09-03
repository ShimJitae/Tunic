using UnityEngine;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] protected Health health;
    [SerializeField] protected GaugeView healthView;

    protected HealthPresenter presenter;

    protected virtual void Awake()
    {
        presenter = new HealthPresenter(health, healthView);
    }

    private void Start()
    {
        presenter.RefreshView();
    }

    private void OnDestroy()
    {
        presenter?.Dispose();
    }
}
