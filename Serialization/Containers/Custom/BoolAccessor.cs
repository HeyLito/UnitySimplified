using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class BoolAccessor : Accessor<bool>
    {
        [SerializeField]
        private bool value;



        public override void Set(bool value) => this.value = value;
        public override void Get(out bool value) => value = this.value;
    }
}