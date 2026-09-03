using System;
using UnityEngine;

[Serializable]
public class StatusPresenter : HealthPresenter
{
    [SerializeField] private readonly Status model;
    [SerializeField] private readonly GaugeView staminaView;

    public StatusPresenter(Status model, GaugeView healthView, GaugeView staminaView)
        : base(model, healthView)
    {
        this.model = model;
        this.staminaView = staminaView;

        model.OnStaminaChanged += HandleStaminaChanged;
    }

    public override void RefreshView()
    {
        base.RefreshView();
        HandleStaminaChanged();
    }

    public override void Dispose()
    {
        base.Dispose();
        model.OnStaminaChanged -= HandleStaminaChanged;
    }

    private void HandleStaminaChanged()
    {
        staminaView.SetGauge(model.CurrStamina, model.MaxStamina);
    }
}
