using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager<T> where T : MonoBehaviour
{
    private readonly IObjectPool<T> pool;

    public PoolManager(T prefab, int defaultCapacity = 10, int maxSize = 20)
    {
        pool = new ObjectPool<T>(
            createFunc: () => Object.Instantiate(prefab),
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => Object.Destroy(obj.gameObject),
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    public T Get => pool.Get();
    public void Release(T obj) => pool.Release(obj);
}
