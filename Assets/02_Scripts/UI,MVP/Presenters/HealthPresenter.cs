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
    }

    private void HandleHealthChanged(float _)
    {
        view.SetGauge(model.CurrHP, model.MaxHP);
    }

    public virtual void Dispose()
    {
        model.OnDamaged -= HandleHealthChanged;
        model.OnRestored -= HandleHealthChanged;
    }

    public virtual void RefreshView()
    {
        view.SetGauge(model.CurrHP, model.MaxHP);
    }
}