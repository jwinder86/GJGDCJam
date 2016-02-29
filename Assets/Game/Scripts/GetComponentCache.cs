using UnityEngine;
using System;
using System.Collections.Generic;

public class GetComponentCache {

    private static GetComponentCache instance;
    public static GetComponentCache Instance {
        get {
            if (instance == null) {
                instance = new GetComponentCache();
            }
            return instance;
        }
    }

    public static void ClearCache() {
        instance = null;
    }

    private Dictionary<Type, Dictionary<Collider, Component>> cache;

    public GetComponentCache() {
        cache = new Dictionary<Type, Dictionary<Collider, Component>>();
    }

    public T GetComponent<T>(Collider obj) where T : Component {
        Type type = typeof(T);
        if (!cache.ContainsKey(type)) {
            cache.Add(type, new Dictionary<Collider, Component>());
        }

        Dictionary<Collider, Component> typeCache = cache[type];
        if (typeCache.ContainsKey(obj)) {
            return (T) typeCache[obj];
        } else {
            Debug.Log("Cache miss, calling GetComponent on: " + obj.name);

            T cmp = obj.GetComponent<T>();
            typeCache.Add(obj, cmp);
            return cmp;
        }
    }
}
