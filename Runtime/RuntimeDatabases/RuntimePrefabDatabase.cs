using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.Collections;

namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimePrefabDatabase : RuntimeDatabase<RuntimePrefabDatabase>
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_prefabsByKeys")]
        private SerializableDictionary<string, GameObject> prefabsByKeys = new();
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_keysByPrefabs")]
        private SerializableDictionary<GameObject, string> keysByPrefabs = new();

        internal IList Items => prefabsByKeys;

        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetAssetsInDirectories(".prefab").Where(asset => asset != null))
                TryAdd((GameObject)asset);
#endif
        }

        public bool Contains(string id) => TryGet(id, out _);
        public bool Contains(GameObject gameObject) => TryGet(gameObject, out _);
        public bool TryGet(string id, out GameObject prefab) => prefabsByKeys.TryGetValue(id, out prefab);
        public bool TryGet(GameObject prefab, out string id) => keysByPrefabs.TryGetValue(prefab, out id);

        internal void Clear()
        {
            if (Application.isPlaying)
                return;

            prefabsByKeys.Clear();
            keysByPrefabs.Clear();
        }
        internal bool TryAdd(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (Contains(prefab))
                return false;

            string id;
            do id = Guid.NewGuid().ToString();
            while (prefabsByKeys.ContainsKey(id));

            prefabsByKeys[id] = prefab;
            keysByPrefabs[prefab] = id;
            return true;
        }
        internal bool TryRemove(string id)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(id, out var prefab))
                return false;

            prefabsByKeys.Remove(id);
            keysByPrefabs.Remove(prefab);
            return true;
        }
        internal bool TryRemove(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(prefab, out string id))
                return false;

            prefabsByKeys.Remove(id);
            keysByPrefabs.Remove(prefab);
            return true;
        }
    }
}