using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyPool : MonoBehaviour
{
    private static EnemyPool instance;
    private readonly Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();

    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return null;
        }

        EnemyPool pool = GetOrCreateInstance();
        int key = prefab.GetInstanceID();
        Queue<GameObject> queue = pool.GetQueue(key);

        while (queue.Count > 0)
        {
            GameObject item = queue.Dequeue();
            if (item == null)
            {
                continue;
            }

            PooledEnemy pooled = item.GetComponent<PooledEnemy>();
            if (pooled != null)
            {
                pooled.InPool = false;
            }

            item.transform.SetParent(null);
            item.transform.SetPositionAndRotation(position, rotation);
            item.SetActive(true);
            return item;
        }

        Object spawned = Instantiate((Object)prefab, position, rotation);
        GameObject created = spawned as GameObject;
        if (created == null && spawned is Component spawnedComponent)
        {
            created = spawnedComponent.gameObject;
        }

        if (created == null)
        {
            Debug.LogWarning($"EnemyPool failed to instantiate prefab '{prefab.name}'.");
            return null;
        }

        PooledEnemy tag = created.GetComponent<PooledEnemy>();
        if (tag == null)
        {
            tag = created.AddComponent<PooledEnemy>();
        }

        tag.PrefabKey = key;
        tag.InPool = false;
        return created;
    }

    public static void Despawn(GameObject instanceObject)
    {
        if (instanceObject == null)
        {
            return;
        }

        PooledEnemy pooled = instanceObject.GetComponent<PooledEnemy>();
        if (pooled == null)
        {
            Destroy(instanceObject);
            return;
        }

        if (pooled.InPool)
        {
            return;
        }

        EnemyPool pool = GetOrCreateInstance();
        Queue<GameObject> queue = pool.GetQueue(pooled.PrefabKey);

        pooled.InPool = true;
        instanceObject.SetActive(false);
        instanceObject.transform.SetParent(pool.transform);
        queue.Enqueue(instanceObject);
    }

    private static EnemyPool GetOrCreateInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject root = new GameObject("EnemyPool");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<EnemyPool>();
        return instance;
    }

    private Queue<GameObject> GetQueue(int key)
    {
        if (!pools.TryGetValue(key, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[key] = queue;
        }

        return queue;
    }
}

public sealed class PooledEnemy : MonoBehaviour
{
    public int PrefabKey { get; set; }
    public bool InPool { get; set; }
}
