using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool
{
    private static ObjectPool instance;
    private Dictionary<string, ObjectPool<GameObject>> objectPools = new Dictionary<string, ObjectPool<GameObject>>();  // 用 Unity 的 ObjectPool 替代 Stack
    private Dictionary<string, Transform> poolParents = new Dictionary<string, Transform>(); // 记录对象池的父物体
    private GameObject poolRoot; // 总对象池的根节点

    public static ObjectPool Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ObjectPool();
            }
            return instance;
        }
    }

    // 获取对象
    public GameObject GetObject(GameObject prefab)
    {
        string key = prefab.name;

        // 确保对象池的根节点存在
        if (poolRoot == null)
        {
            poolRoot = new GameObject("ObjectPool");
        }

        // 确保子池存在
        if (!poolParents.TryGetValue(key, out Transform parent))
        {
            GameObject childPool = new GameObject(key + "Pool");
            childPool.transform.SetParent(poolRoot.transform);
            poolParents[key] = childPool.transform;
            parent = childPool.transform;
        }

        // 如果池子里有对象，直接取出
        if (objectPools.TryGetValue(key, out ObjectPool<GameObject> pool))
        {
            GameObject obj = pool.Get();
            // 如果对象被销毁或无效，创建一个新的对象
            if (obj == null || !obj.activeInHierarchy)
            {
                obj = CreateNewObject(prefab, parent);
            }
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 如果没有对象池，为该 prefab 创建一个新的对象池
            var newPool = new ObjectPool<GameObject>(
                () => CreateNewObject(prefab, parent),  // 创建新对象的方法
                obj => { if (obj != null) obj.SetActive(false); },            // 回收对象的方法
                obj => { if (obj != null) Object.Destroy(obj); }  // 销毁对象的方法
            );
            objectPools[key] = newPool;

            // 获取对象
            return newPool.Get();
        }
    }

    // 创建新对象
    private GameObject CreateNewObject(GameObject prefab, Transform parent)
    {
        GameObject newObj = GameObject.Instantiate(prefab);
        newObj.name = prefab.name; // 避免 "(Clone)" 后缀
        newObj.transform.SetParent(parent);
        return newObj;
    }

    // 回收对象
    public void PushObject(GameObject obj)
    {
        if (obj == null || !obj.activeInHierarchy)
        {
            return;
        }

        string key = obj.name;

        // 查找对应的对象池
        if (objectPools.ContainsKey(key))
        {
            obj.SetActive(false);
            ObjectPool<GameObject> pool = objectPools[key];
            pool.Release(obj);
        }
        else
        {
            UnityEngine.Object.Destroy(obj);  // 如果没有找到池，直接销毁
        }
    }
}

