using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private Bullet bullet;
    public static BulletPool Instance = null;
    public PoolManager<Bullet> pool;

    void Awake()
    {
        Instance = this;
        // pool 생성
        pool = new PoolManager<Bullet>(bullet, 15, 35);
    }
}
