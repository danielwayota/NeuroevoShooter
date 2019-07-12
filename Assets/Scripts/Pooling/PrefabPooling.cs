using System.Collections.Generic;
using UnityEngine;

public class PrefabPooling : MonoBehaviour
{
    // Le singleton
    public static PrefabPooling current;

    public NamedPrefab[] prefabsList;

    private Dictionary<string, GameObject> prefabs;

    private Dictionary<string, List<GameObject>> pools;

    // ===============================================
    void Awake()
    {
        current = this;
    }

    // ===============================================
    void Start()
    {
        this.prefabs = new Dictionary<string, GameObject>();
        this.pools = new Dictionary<string, List<GameObject>>();

        foreach (var namedprfb in this.prefabsList)
        {
            this.prefabs.Add(namedprfb.name, namedprfb.prefab);
            this.pools.Add(namedprfb.name, new List<GameObject>());
        }
    }

    // ===============================================
    public GameObject Instantiate(string name)
    {
        return this.Instantiate(name, Vector3.zero);
    }

    // ===============================================
    public GameObject Instantiate(string name, Vector3 position)
    {
        List<GameObject> pool = this.pools[name];

        // Find already deactivated object to be used
        int i = 0;
        bool found = false;

        GameObject obj = null;

        while (i < pool.Count && !found)
        {
            if (pool[i].activeSelf == false)
            {
                found = true;
                obj = pool[i];
            }

            i++;
        }

        if (obj != null)
        {
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            GameObject prefab = this.prefabs[name];
            obj = Instantiate(prefab, position, Quaternion.identity);
            obj.transform.parent = this.transform;

            // Add to pool
            pool.Add(obj);
            // Probably it's not necesary.
            this.pools[name] = pool;
        }

        return obj;
    }
}

[System.Serializable]
public struct NamedPrefab
{
    public string name;
    public GameObject prefab;
}
