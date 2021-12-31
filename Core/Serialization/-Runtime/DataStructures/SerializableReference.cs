using System;
using UnityEngine;

namespace UnitySimplified.Serialization 
{
    /// <summary>
    /// A unique identifier used to serialize the reference to an object rather than serializing the object itself.
    /// </summary>
    [Serializable]
    public class SerializableReference
    {
        /// <summary>
        /// A string identifier that is only initialized once.
        /// </summary>
        [SerializeField] private string persistentIdentifier;
        [SerializeField] private string typeIndexerAsString;

        public string PersistentIdentifier => persistentIdentifier;
        public string TypeIndexerAsString => typeIndexerAsString;

        internal SerializableReference(Type typeIndexer)
        {
            persistentIdentifier = Guid.NewGuid().ToString();
            typeIndexerAsString = typeIndexer.AssemblyQualifiedName;
        }
        /// <summary>
        /// Creates new instance and organizes it under the <see cref="Type"/> of <paramref name="typeIndexer"/>
        /// </summary>
        /// <param name="obj"></param>
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
        /// Applys <paramref name="referenceObj"/> as the new reference under the current <see cref="PersistentIdentifier"/>.
        /// </summary>
        /// <param name="obj">
        /// </param>
        public void Update(object referenceObj)
        {
            if (string.IsNullOrEmpty(persistentIdentifier))
                return;

            ReferenceStorage.Instance.Append(persistentIdentifier, referenceObj, Type.GetType(TypeIndexerAsString));
        }

        public static void ClearAllOf(Type typeIndexer)
        {

        }

        public override string ToString()
        { return $"{base.ToString()}({(string.IsNullOrEmpty(persistentIdentifier) ? "Empty Identifier" : persistentIdentifier)})"; }
    }
}