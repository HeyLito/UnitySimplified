using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class Vector2Accessor : Accessor<Vector2>
    {
        [SerializeField]
        private float x;
        [SerializeField]
        private float y;



        public override void Set(Vector2 value)
        {
            x = value.x;
            y = value.y;
        }
        public override void Get(out Vector2 value)
        {
            value.x = x;
            value.y = y;
        }
    }
}