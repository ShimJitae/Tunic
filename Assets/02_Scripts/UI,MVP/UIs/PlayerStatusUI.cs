using UnityEngine;

public class PlayerStatusUI : HPBarUI
{
    [SerializeField] protected Status status;
    [SerializeField] protected GaugeView statusView;

    protected override void Awake()
    {
        presenter = new StatusPresenter(status, healthView, statusView);
    }
}
