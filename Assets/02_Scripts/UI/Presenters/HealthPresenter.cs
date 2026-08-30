public class HealthPresenter
{
    private readonly Health model;
    private readonly HealthView view;

    public HealthPresenter(Health model, HealthView view)
    {
        this.model = model;
        this.view = view;

        model.OnDamaged += HandleHealthChanged;
        model.OnRestored += HandleHealthChanged;

        view.SetHealth(model.CurrHP, model.MaxHP);
    }

    private void HandleHealthChanged(float current)
    {
        view.SetHealth(current, model.MaxHP);
    }

    public void Dispose()
    {
        model.OnDamaged -= HandleHealthChanged;
        model.OnRestored -= HandleHealthChanged;
    }
}