using System;
using UnityEngine;

namespace UnitySimplified.Serialization 
{
    [Serializable]
    public class KeyedPrefabsDictionary : SerializableDictionary<string, GameObject> { }
    public class PrefabStorage : Storage<PrefabStorage>
    {
        [SerializeField] 
        private KeyedPrefabsDictionary keyedPrefabs = new KeyedPrefabsDictionary();
        public KeyedPrefabsDictionary KeyedPrefabs => keyedPrefabs;

        public bool TryGetPrefabKey(GameObject gameObject, out string key)
        {
            foreach (var pair in KeyedPrefabs) 
                if (pair.Value == gameObject) 
                {
                    key = pair.Key;
                    return true;
                }
            key = null;
            return false;
        }

        public GameObject RetrieveGameObject(string key)
        {
            KeyedPrefabs.TryGetValue(key, out GameObject result);
            return result;
        }

        public static string FormatGameObjectToKey(GameObject gameObject, int index)
        {
            return gameObject ? $"{gameObject.GetType().Name}.{gameObject.name}.{(index <= -1 ? 0 : index)}" : "";
        }
    }
}