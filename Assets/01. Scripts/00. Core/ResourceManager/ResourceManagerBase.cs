using UnityEngine;
using Core.Singleton;
public class ResourceManagerBase<T> : Singleton<T> where T : MonoBehaviour
{
    public GameObject GetResource(string name)
    {
        var prefab = Resources.Load<GameObject>(name);
        if (prefab == null)
        {
            Debug.LogError($"Resource not found: {name}");
            return null;
        }
        var go = Instantiate(prefab);
        return go;
    }
}
