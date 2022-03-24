using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.Serialization
{
    [Serializable]
    public class KeyedAssetsDictionary : SerializableDictionary<string, UnityObject> { }
    public class AssetStorage : Storage<AssetStorage>
    {
        [SerializeField] private KeyedAssetsDictionary keyedAssets = new KeyedAssetsDictionary();

        private readonly List<Type> _supportedAssets = new List<Type>()
        {
            typeof(Mesh),
            typeof(AudioClip),
            typeof(Material),
            typeof(PhysicMaterial),
            typeof(PhysicsMaterial2D),
            typeof(Flare),
            typeof(GUIStyle),
            typeof(Texture),
            typeof(RuntimeAnimatorController),
            //typeof(AnimatorController),
            typeof(AnimationClip)
        };

        public KeyedAssetsDictionary KeyedAssets => keyedAssets;
        protected override bool SaveAsFile => true;


        public bool StoreAsset(UnityObject assetObject, out string key)
        {
            key = FormatAssetObjectToKey(assetObject, assetObject.name);
            if (string.IsNullOrEmpty(key)) 
                return false;
            KeyedAssets[key] = assetObject;
            return true;
        }
        public UnityObject RetrieveAsset(string key)
        {
            KeyedAssets.TryGetValue(key, out UnityObject result);
            return result;
        }

        public bool SupportsType(Type type)
        {   return _supportedAssets.IndexOf(type) != -1 || type.IsSubclassOf(typeof(ScriptableObject));   }
        public static string FormatAssetObjectToKey(UnityObject assetObject, string name)
        {   return assetObject ? $"{assetObject.GetType().Name}.{name}" : "";   }
    }
}