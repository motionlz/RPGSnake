using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectPool
{
    public string nameId;
    public int poolSize;
    public GameObject poolObject;
}

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance => instance;
    private static ObjectPooling instance;
    public List<ObjectPool> pools = new List<ObjectPool>();
    public Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake() 
    {
        if(instance == null)
            instance = this;
    }

    private void Start() 
    {
        foreach (ObjectPool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.poolSize; i++)
            {
                GameObject obj = Instantiate(pool.poolObject);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDict.Add(pool.nameId, objectPool);
        }
    }

    public GameObject GetFromPool(string nameId,Vector3 position,Quaternion rotation)
    {
        if(!poolDict.ContainsKey(nameId))
        {
            return null;
        }

        GameObject spawnObj = poolDict[nameId].Dequeue();
        spawnObj.SetActive(true);
        spawnObj.transform.position = position;
        spawnObj.transform.rotation = rotation;

        poolDict[nameId].Enqueue(spawnObj);
        return spawnObj;
    }
}
