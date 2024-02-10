using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplified.Serialization 
{
    /// <summary>
    /// A unique identifier used to serialize the reference to an object rather than serializing the object itself.
    /// </summary>
    [Serializable]
    public class ReferenceData
    {
        [SerializeField]
        private string identifier;

        /// <summary>
        /// A string identifier that is only initialized once.
        /// </summary>
        public string Identifier => identifier;

        /// <summary>
        /// Creates new instance and sorts it inside of a <see cref="Dictionary{TKey, TValue}"/>. under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        internal ReferenceData() => identifier = Guid.NewGuid().ToString();
        
        /// <summary>
        /// Creates new instance and adds it into <see cref="RuntimeReferenceDatabase"/>
        /// </summary>
        /// 
        /// <param name="customIdentifier">
        /// The value in <see cref="Identifier"/> is set as this rather than a predetermined Guid identifier.
        /// </param>
        public ReferenceData(string customIdentifier)
        {
            identifier = customIdentifier;
        }
        
        /// <summary>
        /// Creates new instance and sorts it under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        /// <param name="reference"></param>
        public ReferenceData(object reference)
        {
            if (!RuntimeReferenceDatabase.Instance.TryGet(reference, out identifier))
                RuntimeReferenceDatabase.Instance.TryAdd(identifier = Guid.NewGuid().ToString(), reference);
        }

        /// <summary>
        /// Applys <paramref name="referenceObj"/> as the new reference under the current <see cref="Identifier"/>.
        /// </summary>
        /// <param name="referenceObj">
        /// </param>
        public void Update(object referenceObj)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new Exception($"The string identifier under <b>{nameof(identifier)}</b> is empty!");
            RuntimeReferenceDatabase.Instance.TryRemove(identifier);
            RuntimeReferenceDatabase.Instance.TryAdd(identifier, referenceObj);
        }
        public override string ToString() => $"{base.ToString()}({(string.IsNullOrEmpty(identifier) ? "Empty Identifier" : identifier)})";
    }
}