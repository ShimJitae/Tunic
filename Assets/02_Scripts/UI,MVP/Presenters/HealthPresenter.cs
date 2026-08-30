using System;
using UnityEngine;

[Serializable]
public class HealthPresenter : IDisposable
{
    [SerializeField] private readonly Health model;
    [SerializeField] private readonly HealthView view;

    public HealthPresenter(Health model, HealthView view)
    {
        this.model = model;
        this.view = view;

        model.OnDamaged += HandleHealthChanged;
        model.OnRestored += HandleHealthChanged;
    }

    private void HandleHealthChanged(float _)
    {
        view.SetHealth(model.CurrHP, model.MaxHP);
    }

    public void Dispose()
    {
        model.OnDamaged -= HandleHealthChanged;
        model.OnRestored -= HandleHealthChanged;
    }
}