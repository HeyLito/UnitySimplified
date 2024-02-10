using System;
using UnityEngine;

namespace UnitySimplified.VariableReferences
{
    [Serializable]
    public class VariableReference<TValue, TReference> where TReference : IVariableObjectReference<TValue>
    {
        [SerializeField]
        private bool _useConstant;
        [SerializeField]
        private TValue _constantValue;
        [SerializeField]
        private TReference _reference;

        public bool UseConstant { get => _useConstant; protected set => _useConstant = value; }
        public TValue ConstantValue { get => _constantValue; protected set => _constantValue = value; }
        public TReference Reference { get => _reference; protected set => _reference = value; }

        public VariableReference() => _useConstant = true;
        public VariableReference(TValue value) : this() => _constantValue = value;

        public TValue Value
        {
            get => _useConstant || _reference == null ? _constantValue : _reference.GetValue();
            set
            {
                if (_useConstant || _reference == null)
                    _constantValue = value;
                else _reference.SetValue(value);
            }
        }
    }
    [Serializable]
    public class VariableReference<T> : VariableReference<T, VariableObjectReference<T>>
    {
        public VariableReference() { }
        public VariableReference(T value) : base(value) { }
    }
}