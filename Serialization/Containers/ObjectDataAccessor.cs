using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class ObjectDataAccessor : Accessor<ObjectData>
    {
        [SerializeField]
        private string targetType;
        [SerializeField]
        private string reference;
        [SerializeField]
        private AccessorDictionary accessors;



        public override void Set(ObjectData value)
        {
            targetType = value.targetType;
            reference = value.reference;
            accessors = value.accessors;
        }
        public override void Get(out ObjectData value) => value = new ObjectData(targetType, new ReferenceData(reference), accessors);
    }
}