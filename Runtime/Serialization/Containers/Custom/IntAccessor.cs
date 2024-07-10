using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class IntAccessor : Accessor<int>
    {
        [SerializeField]
        private int value;



        public override void Set(int value) => this.value = value;
        public override void Get(out int value) => value = this.value;
    }
}