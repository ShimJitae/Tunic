using System;

public interface IAttackZoneController
{
    event Action<bool> OnAttackZoneChanged;

    void SetAttackZoneActive(bool isActive);
}
