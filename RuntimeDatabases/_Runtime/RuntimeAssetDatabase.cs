using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnitySimplified.Collections;
#if UNITY_EDITOR
using UnitySimplifiedEditor.RuntimeDatabases;
#endif
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimeAssetDatabase : RuntimeDatabase<RuntimeAssetDatabase>
    {
        [SerializeField, HideInInspector]
        private SerializableDictionary<string, UnityObject> _assetsByKeys = new();
        [SerializeField, HideInInspector]
        private SerializableDictionary<UnityObject, string> _keysByAssets = new();


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
            typeof(Material),
            typeof(PhysicMaterial),
            typeof(PhysicsMaterial2D),
            typeof(Flare),
            typeof(GUIStyle),
            typeof(Texture),
            typeof(RuntimeAnimatorController),
            typeof(AnimationClip)
        };
        
        
        internal IList Items => _assetsByKeys;


        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in RuntimeDatabaseEditorUtility.GetBuiltInAssets().Where(asset => asset != null))
                TryAdd(asset);
            foreach (var asset in RuntimeDatabaseEditorUtility.GetAssetsInDirectories(ValidatedAssetExtensions).Where(asset => asset != null))
                TryAdd(asset);
#endif
        }

        public bool Contains(string id) => TryGet(id, out _);
        public bool Contains(UnityObject asset) => TryGet(asset, out _);
        public bool TryGet(string id, out UnityObject asset) => _assetsByKeys.TryGetValue(id, out asset);
        public bool TryGet(UnityObject asset, out string id) => _keysByAssets.TryGetValue(asset, out id);
        public bool SupportsType(Type type) => type.IsSubclassOf(typeof(ScriptableObject)) || ValidatedAssetTypes.Any(supportedAssetType => supportedAssetType.IsAssignableFrom(type));



        internal void Clear()
        {
            if (Application.isPlaying)
                return;

            _assetsByKeys.Clear();
            _keysByAssets.Clear();
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
            while (_assetsByKeys.ContainsKey(id));

            _assetsByKeys[id] = asset;
            _keysByAssets[asset] = id;

            return true;
        }
        internal bool TryRemove(string id)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(id, out UnityObject asset))
                return false;

            _assetsByKeys.Remove(id);
            _keysByAssets.Remove(asset);
            return true;
        }
        internal bool TryRemove(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;
            if (!TryGet(asset, out string id))
                return false;

            _assetsByKeys.Remove(id);
            _keysByAssets.Remove(asset);
            return true;
        }
    }
}