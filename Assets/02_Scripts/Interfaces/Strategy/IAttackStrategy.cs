using UnityEngine;

public interface IAttackStrategy
{
    public Weapon Weapon { get; set; }
    public void Attack();
}
