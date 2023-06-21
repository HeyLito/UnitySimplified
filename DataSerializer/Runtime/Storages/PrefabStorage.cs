using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization 
{
    [Serializable]
    public class PrefabsByKeysDictionary : SerializableDictionary<string, GameObject> { }
    [Serializable]
    public class KeysByPrefabsDictionary : SerializableDictionary<GameObject, string> { }

    public class PrefabStorage : Storage<PrefabStorage>
    {
        [SerializeField] 
        private PrefabsByKeysDictionary _prefabsByKeys = new PrefabsByKeysDictionary();
        [SerializeField]
        private KeysByPrefabsDictionary _keysByPrefabs = new KeysByPrefabsDictionary();

        public List<KeyValuePair<string, GameObject>> Values => _prefabsByKeys.ToList();

        public bool ContainsPrefab(GameObject gameObject) => ContainsPrefab(gameObject, out _);
        public bool ContainsPrefab(GameObject gameObject, out string id)
        {
            if (!_keysByPrefabs.TryGetValue(gameObject, out id))
            {
                id = null;
                return false;
            }
            else return true;
        }

        public bool ContainsID(string id) => ContainsID(id, out _);
        public bool ContainsID(string id, out GameObject prefab)
        {
            if (!_prefabsByKeys.TryGetValue(id, out prefab))
            {
                prefab = null;
                return false;
            }
            else return true;
        }

        public void Clear()
        {
            if (Application.isPlaying)
                return;

            _prefabsByKeys.Clear();
            _keysByPrefabs.Clear();
        }
        public bool InsertPrefabEntry(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;

            if (ContainsPrefab(prefab))
                return false;
            else
            {
                string id;
                do id = Guid.NewGuid().ToString();
                while (_prefabsByKeys.ContainsKey(id));

                _prefabsByKeys[id] = prefab;
                _keysByPrefabs[prefab] = id;

                return true;
            }
        }
        public bool RemovePrefabEntry(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;

            if (ContainsPrefab(prefab, out string id))
            {
                _prefabsByKeys.Remove(id);
                _keysByPrefabs.Remove(prefab);
                return true;
            }
            else return false;
        }

        public static string FormatGameObjectToKey(GameObject gameObject, int index) => gameObject ? $"{gameObject.GetType().Name}.{gameObject.name}.{(index <= -1 ? 0 : index)}" : "";
    }
}