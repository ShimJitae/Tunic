using UnityEngine;

public class PlayerAttackModule : MonoBehaviour, IAttackStrategy
{
    public Weapon Weapon { get; set; }

    private void Awake()
    {
        Weapon = transform.GetComponentInChildren<Weapon>();
        if (Weapon == null)
        {
            Debug.LogError($"PlayerAttackModule : {gameObject.name}의 하위 컴포넌트에서 Weapon을 발견하지 못했습니다.");
        }
    }

    public void Attack()
    {
    }
}
