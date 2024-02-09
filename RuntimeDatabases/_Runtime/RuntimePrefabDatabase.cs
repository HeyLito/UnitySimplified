using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnitySimplified.Serialization;
#if UNITY_EDITOR
using UnitySimplifiedEditor.RuntimeDatabases;
#endif

namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimePrefabDatabase : RuntimeDatabase<RuntimePrefabDatabase>
    {
        [SerializeField, HideInInspector] 
        private SerializableDictionary<string, GameObject> _prefabsByKeys = new();
        [SerializeField, HideInInspector]
        private SerializableDictionary<GameObject, string> _keysByPrefabs = new();


        internal IList Items => _prefabsByKeys;



        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in RuntimeDatabaseEditorUtility.GetAssetsInDirectories(".prefab").Where(asset => asset != null))
                TryAdd((GameObject)asset);
#endif
        }

        public bool Contains(string id) => TryGet(id, out _);
        public bool Contains(GameObject gameObject) => TryGet(gameObject, out _);
        public bool TryGet(string id, out GameObject prefab) => _prefabsByKeys.TryGetValue(id, out prefab);
        public bool TryGet(GameObject prefab, out string id) => _keysByPrefabs.TryGetValue(prefab, out id);

        internal void Clear()
        {
            if (Application.isPlaying)
                return;

            _prefabsByKeys.Clear();
            _keysByPrefabs.Clear();
        }
        internal bool TryAdd(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (Contains(prefab))
                return false;

            string id;
            do id = Guid.NewGuid().ToString();
            while (_prefabsByKeys.ContainsKey(id));

            _prefabsByKeys[id] = prefab;
            _keysByPrefabs[prefab] = id;
            return true;
        }
        internal bool TryRemove(string id)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(id, out var prefab))
                return false;
            
            _prefabsByKeys.Remove(id);
            _keysByPrefabs.Remove(prefab);
            return true;
        }
        internal bool TryRemove(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(prefab, out string id))
                return false;

            _prefabsByKeys.Remove(id);
            _keysByPrefabs.Remove(prefab);
            return true;
        }
    }
}