using System.Collections.Generic;
using UnityEngine;
using Core.Singleton;
public class ResourceManagerBase<T> : Singleton<T> where T : MonoBehaviour
{
    private Dictionary<string, GameObject> _cache = new Dictionary<string, GameObject>();
    public GameObject GetPrefab(string name)
    {
        if(_cache.ContainsKey(name))
            return _cache[name];
        
        var prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
        {
            Debug.LogError($"Resource not found: {name}");
            return null;
        }
        _cache.Add(name, prefab);

        return prefab;
    }
}
