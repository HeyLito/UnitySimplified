using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entry = UnitySimplified.RuntimeDatabases.IRuntimeValueDatabase<UnityEngine.GameObject>.Entry;

namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimePrefabDatabase : RuntimeDatabase<RuntimePrefabDatabase>, IRuntimeValueDatabase<GameObject>, IEnumerable<Entry>
    {
        [SerializeField, HideInInspector]
        private List<Entry> entries = new();

        private ILookup<string, GameObject> _prefabsByIdentifier;
        private ILookup<GameObject, string> _identifiersByPrefab;
        
        List<Entry> IRuntimeValueDatabase<GameObject>.Entries => entries;

        protected override void OnCreate()
        {
#if UNITY_EDITOR
            foreach (var asset in UnitySimplifiedEditor.RuntimeDatabases.RuntimeDatabaseUtility.GetAssetsInDirectories(".prefab").Where(asset => asset != null))
                TryAdd((GameObject)asset);

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

        public bool Contains(string identifier) => TryGet(identifier, out _);
        public bool Contains(GameObject gameObject) => TryGet(gameObject, out _);
        public bool TryGet(string identifier, out GameObject prefab)
        {
            CheckCache();
            prefab = null;
            var result = _prefabsByIdentifier[identifier];
            if (result.Equals(default(IEnumerable<GameObject>)))
                return false;
            try { prefab = result.First(); }
            catch (InvalidOperationException) { return false; }
            return true;
        }
        public bool TryGet(GameObject prefab, out string identifier)
        {
            CheckCache();
            identifier = null;
            var result = _identifiersByPrefab[prefab];
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
        internal bool TryAdd(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (Contains(prefab))
                return false;

            string identifier;
            do identifier = Guid.NewGuid().ToString();
            while (Contains(identifier));

            entries.Add(new Entry(identifier, prefab));
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
        internal bool TryRemove(GameObject prefab)
        {
            if (Application.isPlaying)
                return false;
            if (!Contains(prefab))
                return false;

            entries.RemoveAll(x => x.Value.Equals(prefab));
            RedoCache();
            return true;
        }

        private void CheckCache()
        {
            if (_prefabsByIdentifier != null && _identifiersByPrefab != null)
                return;
            RedoCache();
        }
        private void RedoCache()
        {
            _prefabsByIdentifier = entries.ToLookup(x => x.Identifier, y => y.Value);
            _identifiersByPrefab = entries.ToLookup(x => x.Value, y => y.Identifier);
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Entry>)this).GetEnumerator();
        IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator() => entries.GetEnumerator();
    }
}