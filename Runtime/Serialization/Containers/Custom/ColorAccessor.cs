using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class ColorAccessor : Accessor<Color>
    {
        [SerializeField]
        private float r;
        [SerializeField]
        private float g;
        [SerializeField]
        private float b;
        [SerializeField]
        private float a;



        public override void Set(Color value)
        {
            r = value.r;
            g = value.g;
            b = value.b;
            a = value.a;
        }
        public override void Get(out Color value)
        {
            value.r = r;
            value.g = g;
            value.b = b;
            value.a = a;
        }
    }
}