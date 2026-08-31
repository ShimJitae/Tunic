using System;
using UnityEngine;

public interface IAttackStrategy
{
    public event Action OnAttack;
    public Weapon Weapon { get; }
    public void Attack();
    public void ActiveAttackZone(bool enable);
}
