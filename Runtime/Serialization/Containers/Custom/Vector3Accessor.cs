using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class Vector3Accessor : Accessor<Vector3>
    {
        [SerializeField]
        private float x;
        [SerializeField]
        private float y;
        [SerializeField]
        private float z;



        public override void Set(Vector3 value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
        public override void Get(out Vector3 value)
        {
            value.x = x;
            value.y = y;
            value.z = z;
        }
    }
}