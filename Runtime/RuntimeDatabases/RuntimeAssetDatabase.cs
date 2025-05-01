using System;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.Collections;
using UnityObject = UnityEngine.Object;

[assembly: InternalsVisibleTo("UnitySimplified.Editor")]
namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimeAssetDatabase : RuntimeDatabase<RuntimeAssetDatabase>
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_assetsByKeys")]
        private SerializableDictionary<string, UnityObject> assetsByKeys = new();
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_keysByAssets")]
        private SerializableDictionary<UnityObject, string> keysByAssets = new();


        public string[] ValidatedAssetExtensions => new[]
        {
            ".prefab",
            ".asset",
            ".fbx",
            ".mat",
            ".png",
            ".mp4",
            ".mixer",
            ".ogg",
            ".controller",
        };
        public Type[] ValidatedAssetTypes => new[]
        {
            typeof(Mesh),
            typeof(AudioClip),
            typeof(AnimationClip),
            typeof(Material),
            typeof(Flare),
            typeof(Texture),
            typeof(RuntimeAnimatorController),
            typeof(PhysicsMaterial2D),
#if UNITY_6000_0_OR_NEWER
            typeof(PhysicsMaterial),
#else
            typeof(PhysicMaterial),
#endif
        };


        internal IList Items => assetsByKeys;

        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetBuiltInAssets().Where(asset => asset != null))
                TryAdd(asset);
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetAssetsInDirectories(ValidatedAssetExtensions).Where(asset => asset != null))
                TryAdd(asset);
#endif
        }

        public bool Contains(string id) => TryGet(id, out _);
        public bool Contains(UnityObject asset) => TryGet(asset, out _);
        public bool TryGet(string id, out UnityObject asset) => assetsByKeys.TryGetValue(id, out asset);
        public bool TryGet(UnityObject asset, out string id) => keysByAssets.TryGetValue(asset, out id);
        public bool SupportsType(Type type) => type.IsSubclassOf(typeof(ScriptableObject)) || ValidatedAssetTypes.Any(supportedAssetType => supportedAssetType.IsAssignableFrom(type));

        internal void Clear()
        {
            if (Application.isPlaying)
                return;

            assetsByKeys.Clear();
            keysByAssets.Clear();
        }
        internal bool TryAdd(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;
            if (Contains(asset))
                return false;
            if (!SupportsType(asset.GetType()))
                return false;

            string id;
            do id = Guid.NewGuid().ToString();
            while (assetsByKeys.ContainsKey(id));

            assetsByKeys[id] = asset;
            keysByAssets[asset] = id;

            return true;
        }
        internal bool TryRemove(string id)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(id, out UnityObject asset))
                return false;

            assetsByKeys.Remove(id);
            keysByAssets.Remove(asset);
            return true;
        }
        internal bool TryRemove(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(asset, out string id))
                return false;

            assetsByKeys.Remove(id);
            keysByAssets.Remove(asset);
            return true;
        }
    }
}