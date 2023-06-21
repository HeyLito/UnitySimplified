using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.Serialization;

namespace UnitySimplified
{
    public class ReferenceStorage : Storage<ReferenceStorage>
    {
        [SerializeField] private Dictionary<Type, ValueTuple<Dictionary<string, object>, Dictionary<object, string>>> _wrappedreferences = new Dictionary<Type, ValueTuple<Dictionary<string, object>, Dictionary<object, string>>>();

        public Dictionary<Type, ValueTuple<Dictionary<string, object>, Dictionary<object, string>>> WrappedReferences => _wrappedreferences;

        private Dictionary<string, object> GetReferencesByIdentifiers(Type typeIndexer, bool autoConstruct = true)
        {
            var refs = GetReferences(typeIndexer, autoConstruct);
            if (!autoConstruct && refs == default(ValueTuple<Dictionary<string, object>, Dictionary<object, string>>))
                return null;
            else return refs.Item1;
        }
        private Dictionary<object, string> GetReferencesByObjects(Type typeIndexer, bool autoConstruct = true)
        {
            var refs = GetReferences(typeIndexer, autoConstruct);
            if (!autoConstruct && refs == default(ValueTuple<Dictionary<string, object>, Dictionary<object, string>>))
                return null;
            else return refs.Item2;
        }
        private ValueTuple<Dictionary<string, object>, Dictionary<object, string>> GetReferences(Type typeIndexer, bool autoConstruct = true)
        {
            if (!_wrappedreferences.TryGetValue(typeIndexer, out (Dictionary<string, object>, Dictionary<object, string>) unwrappedReferences))
            {
                if (!autoConstruct)
                    return default;
                unwrappedReferences = (new Dictionary<string, object>(), new Dictionary<object, string>());
                _wrappedreferences[typeIndexer] = unwrappedReferences;
            }
            return unwrappedReferences;
        }

        public bool Contains(ReferenceData referenceData) => Contains(referenceData.PersistentIdentifier, Type.GetType(referenceData.TypeIndexerAsString));
        public bool Contains(string persistentID, Type typeIndexer) => TryGetObject(persistentID, typeIndexer, out _);
        public bool TryGetObject(string persistentID, Type typeIndexer, out object obj)
        {
            var refsByIdentifiers = GetReferencesByIdentifiers(typeIndexer, false);
            if (refsByIdentifiers == null || !refsByIdentifiers.TryGetValue(persistentID, out obj))
            {
                obj = null;
                return false;
            }
            return true;
        }

        public bool Contains(object obj, Type typeIndexer) => TryGetIdentifier(obj, typeIndexer, out _);
        public bool TryGetIdentifier(object obj, Type typeIndexer, out string id)
        {
            var refsByObjects = GetReferencesByObjects(typeIndexer, false);
            if (refsByObjects == null || !refsByObjects.TryGetValue(obj, out id))
            {
                id = null;
                return false;
            }
            return true;
        }

        public void Append(ReferenceData referenceData, object obj)
        {

        }

        public void Append(string persistanceID, object obj, Type typeIndexer)
        {
            var refs = GetReferences(typeIndexer);
            refs.Item1[persistanceID] = obj;
            refs.Item2[obj] = persistanceID;
        }

        public void SetIdentifer(string id, object obj, Type typeIndexer)
        {
            var refsByObjects = GetReferencesByObjects(typeIndexer, false);
            if (refsByObjects != null)
                refsByObjects[obj] = id;
        }
        public void SetObject(string id, object obj, Type typeIndexer)
        {
            var refsByIdentifiers = GetReferencesByIdentifiers(typeIndexer, false);
            if (refsByIdentifiers != null)
                refsByIdentifiers[id] = obj;
        }
    }
}