using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Entry = UnitySimplified.RuntimeDatabases.IRuntimeValueDatabase<UnityEngine.Object>.Entry;

[assembly: InternalsVisibleTo("UnitySimplified.Editor")]
namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimeAssetDatabase : RuntimeDatabase<RuntimeAssetDatabase>, IRuntimeValueDatabase<UnityObject>,  IEnumerable<Entry>
    {
        [SerializeField, HideInInspector]
        private List<Entry> entries = new();

        private ILookup<string, UnityObject> _assetsByIdentifier;
        private ILookup<UnityObject, string> _identifiersByAsset;

        List<Entry> IRuntimeValueDatabase<UnityObject>.Entries => entries;
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

        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetBuiltInAssets().Where(asset => asset != null))
                TryAdd(asset);
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetAssetsInDirectories(ValidatedAssetExtensions).Where(asset => asset != null))
                TryAdd(asset);

            int Sort(Entry lhs, Entry rhs)
            {
                bool hasLhs = lhs.Value != null;
                bool hasRhs = rhs.Value != null;

                if (!hasLhs && !hasRhs)
                    return 0;
                if (!hasLhs)
                    return -1;
                if (!hasRhs)
                    return 1;
                return string.Compare(lhs.Value.name, rhs.Value.name, StringComparison.Ordinal);
            }
            entries.Sort(Comparer<Entry>.Create(Sort));
#endif
        }

        public bool SupportsType(Type type) => type.IsSubclassOf(typeof(ScriptableObject)) || ValidatedAssetTypes.Any(supportedAssetType => supportedAssetType.IsAssignableFrom(type));
        public bool Contains(string identifier) => TryGet(identifier, out _);
        public bool Contains(UnityObject asset) => TryGet(asset, out _);
        public bool TryGet(string identifier, out UnityObject asset)
        {
            CheckCache();
            asset = null;
            var result = _assetsByIdentifier[identifier];
            if (result.Equals(default(IEnumerable<GameObject>)))
                return false;
            try { asset = result.First(); }
            catch (InvalidOperationException) { return false; }
            return true;
        }
        public bool TryGet(UnityObject asset, out string identifier)
        {
            CheckCache();
            identifier = null;
            var result = _identifiersByAsset[asset];
            if (result.Equals(default(IEnumerable<string>)))
                return false;
            try { identifier = result.First(); }
            catch (InvalidOperationException) { return false; }
            return true;
        }

        internal void Clear()
        {
            if (Application.isPlaying)
                return;

            entries.Clear();
            RedoCache();
        }
        internal bool TryAdd(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;
            if (Contains(asset))
                return false;
            if (!SupportsType(asset.GetType()))
                return false;

            string identifier;
            do identifier = Guid.NewGuid().ToString();
            while (Contains(identifier));

            entries.Add(new Entry(identifier, asset));
            RedoCache();
            return true;
        }
        internal bool TryRemove(string identifier)
        {
            if (Application.isPlaying)
                return false;
            if (!Contains(identifier))
                return false;

            entries.RemoveAll(x => x.Identifier.Equals(identifier));
            RedoCache();
            return true;
        }
        internal bool TryRemove(UnityObject asset)
        {
            if (Application.isPlaying)
                return false;
            if (!Contains(asset))
                return false;

            entries.RemoveAll(x => x.Value.Equals(asset));
            RedoCache();
            return true;
        }

        private void CheckCache()
        {
            if (_assetsByIdentifier != null && _identifiersByAsset != null)
                return;
            RedoCache();
        }
        private void RedoCache()
        {
            _assetsByIdentifier = entries.ToLookup(x => x.Identifier, y => y.Value);
            _identifiersByAsset = entries.ToLookup(x => x.Value, y => y.Identifier);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Entry>)this).GetEnumerator();
        IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator() => entries.GetEnumerator();
    }
}