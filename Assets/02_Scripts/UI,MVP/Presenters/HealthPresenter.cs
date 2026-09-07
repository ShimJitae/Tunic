using System;
using UnityEngine;

[Serializable]
public class HealthPresenter : IDisposable
{
    [SerializeField] private readonly Health model;
    [SerializeField] private readonly GaugeView view;

    public HealthPresenter(Health model, GaugeView view)
    {
        this.model = model;
        this.view = view;

        model.OnDamaged += HandleHealthChanged;
        model.OnRestored += HandleHealthChanged;
        model.OnDied += HandleDied;
    }

    private void HandleHealthChanged(float _)
    {
        view.SetGauge(model.CurrHP, model.MaxHP);
    }

    private void HandleDied()
    {
        if (model.gameObject.CompareTag("Enemy"))
            view.gameObject.SetActive(false);
    }


    public virtual void Dispose()
    {
        model.OnDamaged -= HandleHealthChanged;
        model.OnRestored -= HandleHealthChanged;
        model.OnDied -= HandleDied;
    }

    public virtual void RefreshView()
    {
        view.SetGauge(model.CurrHP, model.MaxHP);
    }
}