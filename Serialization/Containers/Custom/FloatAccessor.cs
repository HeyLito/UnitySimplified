using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class FloatAccessor : Accessor<float>
    {
        [SerializeField]
        private float value;



        public override void Set(float value) => this.value = value;
        public override void Get(out float value) => value = this.value;
    }
}