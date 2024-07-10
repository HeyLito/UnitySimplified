using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("UnitySimplified.Editor", AllInternalsVisible = true)]

namespace UnitySimplified.RuntimeDatabases
{
    public class RuntimeReferenceDatabase : RuntimeDatabase<RuntimeReferenceDatabase>
    {
        private readonly Dictionary<string, object> _referencesByIdentifiers = new();
        private readonly Dictionary<object, string> _identifiersByReferences = new();

        public bool Contains(string identifier) => TryGet(identifier, out _);
        public bool Contains(object reference) => TryGet(reference, out _);
        public bool TryGet(string identifier, out object reference) => _referencesByIdentifiers.TryGetValue(identifier, out reference);
        public bool TryGet(object reference, out string identifier) => _identifiersByReferences.TryGetValue(reference, out identifier);

        internal bool TryAdd(string identifier, object reference)
        {
            if (!Application.isPlaying)
                return false;
            if (Contains(reference))
                return false;

            _referencesByIdentifiers.Add(identifier, reference);
            _identifiersByReferences.Add(reference, identifier);
            return true;
        }

        internal bool TryRemove(string identifier)
        {
            if (!Application.isPlaying)
                return false;
            if (!TryGet(identifier, out var reference))
                return false;
            if (reference != null)
                _identifiersByReferences.Remove(reference);
            return true;
        }
    }
}