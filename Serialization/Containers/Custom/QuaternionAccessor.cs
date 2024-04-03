using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class QuaternionAccessor : Accessor<Quaternion>
    {
        [SerializeField]
        private float x;
        [SerializeField]
        private float y;
        [SerializeField]
        private float z;
        [SerializeField]
        private float w;



        public override void Set(Quaternion value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
            w = value.w;
        }
        public override void Get(out Quaternion value)
        {
            value.x = x;
            value.y = y;
            value.z = z;
            value.w = w;
        }
    }
}