using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization 
{
    /// <summary>
    /// A unique identifier used to serialize the reference to an object rather than serializing the object itself.
    /// </summary>
    [Serializable]
    public class SerializableReference
    {
        [SerializeField] private string persistentIdentifier;
        [SerializeField] private string typeIndexerAsString;

        /// <summary>
        /// A string identifier that is only initialized once.
        /// </summary>
        public string PersistentIdentifier => persistentIdentifier;
        /// <summary>
        /// The string used as a Key in a <see cref="Dictionary{TKey, TValue}"/> in which the reference is stored in.
        /// </summary>
        public string TypeIndexerAsString => typeIndexerAsString;

        /// <summary>
        /// Creates new instance and sorts it inside of a <see cref="Dictionary{TKey, TValue}"/>. under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        /// <param name="typeIndexer">
        /// The type-value in which is used to store inside of a <see cref="Dictionary{TKey, TValue}"/>
        /// </param>
        internal SerializableReference(Type typeIndexer)
        {
            persistentIdentifier = Guid.NewGuid().ToString();
            typeIndexerAsString = typeIndexer.AssemblyQualifiedName;
        }
        /// <summary>
        /// Creates new instance and sorts it under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        /// <param name="referenceObj"></param>
        /// <param name="typeIndexer"></param>
        public SerializableReference(object referenceObj, Type typeIndexer)
        {
            persistentIdentifier = Guid.NewGuid().ToString();
            typeIndexerAsString = typeIndexer.AssemblyQualifiedName;

            if (ReferenceStorage.Instance.TryGetIdentifier(referenceObj, typeIndexer, out string id))
            {
                persistentIdentifier = id;
                ReferenceStorage.Instance.SetObject(id, referenceObj, typeIndexer);
            }
            else ReferenceStorage.Instance.Append(persistentIdentifier, referenceObj, typeIndexer);
        }

        /// <summary>
        /// Creates new instance and sorts it under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        /// <param name="customIdentifier">
        /// The value in <see cref="persistentIdentifier"/> is set as this rather than a predetermined Guid identifier.
        /// </param>
        /// <param name="typeIndexer"></param>
        public SerializableReference(string customIdentifier, Type typeIndexer)
        {
            persistentIdentifier = customIdentifier;
            typeIndexerAsString = typeIndexer.AssemblyQualifiedName;
        }

        /// <summary>
        /// Applys <paramref name="referenceObj"/> as the new reference under the current <see cref="PersistentIdentifier"/>.
        /// </summary>
        /// <param name="referenceObj">
        /// </param>
        public void Update(object referenceObj)
        {
            if (string.IsNullOrEmpty(persistentIdentifier))
                throw new Exception($"The string identifier under <b>{nameof(persistentIdentifier)}</b> is empty!");

            ReferenceStorage.Instance.Append(persistentIdentifier, referenceObj, Type.GetType(TypeIndexerAsString));
        }

        public static void ClearAllOf(Type typeIndexer)
        {

        }

        public override string ToString()
        { return $"{base.ToString()}({(string.IsNullOrEmpty(persistentIdentifier) ? "Empty Identifier" : persistentIdentifier)})"; }
    }
}