using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic MonoBehaviour object pool.
/// Attach to a manager GameObject and call Get() / Release() instead of Instantiate / Destroy.
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly Queue<T> _pool = new();

    public ObjectPool(T prefab, int initialSize = 10, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent;

        for (int i = 0; i < initialSize; i++)
            _pool.Enqueue(CreateInstance());
    }

    public T Get()
    {
        T instance = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();
        instance.gameObject.SetActive(true);
        return instance;
    }

    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
    }

    public void ReleaseAll(IEnumerable<T> instances)
    {
        foreach (T instance in instances)
            Release(instance);
    }

    private T CreateInstance()
    {
        T instance = Object.Instantiate(_prefab, _parent);
        instance.gameObject.SetActive(false);
        return instance;
    }
}
