using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class Vector3IntAccessor : Accessor<Vector3Int>
    {
        [SerializeField]
        private int x;
        [SerializeField]
        private int y;
        [SerializeField]
        private int z;



        public override void Set(Vector3Int value)
        {
            x = value.x;
            y = value.y;
            z = value.z;
        }
        public override void Get(out Vector3Int value)
        {
            value = Vector3Int.zero;
            value.x = x;
            value.y = y;
            value.z = z;
        }
    }
}