using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitySimplified.VariableReferences
{
    [Serializable]
    public class VariableReference<TValue, TReference> : IVariableReference<TValue, TReference> where TReference : IVariableAsset<TValue>
    {
        [SerializeField]
        [FormerlySerializedAs("_useConstant")]
        private bool valueToggle;
        [SerializeField]
        [FormerlySerializedAs("_constantValue")]
        private TValue constant;
        [SerializeField]
        [FormerlySerializedAs("_reference")]
        private TReference reference;

        public bool ValueToggle { get => valueToggle; protected set => valueToggle = value; }
        public TValue Constant { get => constant; protected set => constant = value; }
        public TReference Reference { get => reference; protected set => reference = value; }

        public VariableReference() => valueToggle = true;
        public VariableReference(TValue value) : this() => constant = value;

        public TValue Value
        {
            get => valueToggle || reference == null ? constant : reference.GetValue();
            set
            {
                if (valueToggle || reference == null)
                    constant = value;
                else reference.SetValue(value);
            }
        }
    }
    [Serializable]
    public class VariableReference<T> : VariableReference<T, VariableAsset<T>>
    {
        public VariableReference() { }
        public VariableReference(T value) : base(value) { }
    }
}