using System;
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
        /// A string identifier that is only set during a constructor call.
        /// </summary>
        public string Identifier => identifier;


        internal ReferenceData() => identifier = Guid.NewGuid().ToString();

        /// <summary>
        /// Creates new instance and adds it into <see cref="RuntimeReferenceDatabase"/>
        /// </summary>
        /// 
        /// <param name="customIdentifier">
        /// Sets <paramref name="customIdentifier"/> to <see cref="Identifier"/> instead of using a random generated <see cref="Guid"/> value.
        /// </param>
        public ReferenceData(string customIdentifier) => identifier = customIdentifier;

        /// <summary>
        /// Creates new instance and adds it into <see cref="RuntimeReferenceDatabase"/>
        /// </summary>
        /// 
        /// <param name="reference">
        /// The actual reference value given when looking up <see cref="Identifier"/> in <see cref="RuntimeReferenceDatabase"/>
        /// <br/>
        /// Initializes <see cref="Identifier"/> with <see cref="Guid.NewGuid"/> if lookup was unsuccessful.
        /// </param>
        public ReferenceData(object reference)
        {
            if (!RuntimeReferenceDatabase.Instance.TryGet(reference, out identifier))
                RuntimeReferenceDatabase.Instance.TryAdd(identifier = Guid.NewGuid().ToString(), reference);
        }



        /// <summary>
        /// Lookup <see cref="Identifier"/> in <see cref="RuntimeReferenceDatabase"/> and overwrite the lookup result with <paramref name="reference"/>.
        /// </summary>
        /// <param name="reference">
        /// </param>
        public void Update(object reference)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException($"The string identifier under <b>{nameof(identifier)}</b> is empty!");
            RuntimeReferenceDatabase.Instance.TryRemove(identifier);
            RuntimeReferenceDatabase.Instance.TryAdd(identifier, reference);
        }

        public bool TryGet(out object reference)
        {
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException($"The string identifier under <b>{nameof(identifier)}</b> is empty!");
            return RuntimeReferenceDatabase.Instance.TryGet(identifier, out reference);
        }

        public override string ToString() => $"{base.ToString()}({(string.IsNullOrEmpty(identifier) ? "Empty Identifier" : identifier)})";

        public static implicit operator string(ReferenceData referenceData) => referenceData?.identifier;
    }
}