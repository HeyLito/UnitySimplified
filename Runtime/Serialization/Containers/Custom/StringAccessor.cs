using System;
using UnityEngine;

namespace UnitySimplified.Serialization.Containers
{
    [Serializable]
    public sealed class StringAccessor : Accessor<string>
    {
        [SerializeField]
        private string value;



        public override void Set(string value) => this.value = value;
        public override void Get(out string value) => value = this.value;
    }
}