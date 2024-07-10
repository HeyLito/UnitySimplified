using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class Vector2IntAccessor : Accessor<Vector2Int>
    {
        [SerializeField]
        private int x;
        [SerializeField]
        private int y;



        public override void Set(Vector2Int value)
        {
            x = value.x;
            y = value.y;
        }
        public override void Get(out Vector2Int value)
        {
            value = default;
            value.x = x;
            value.y = y;
        }
    }
}