using System;
using System.Reflection;
using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public abstract class Parameter : AnimationCondition
    {
        [SerializeField]
        private KeywordReference _nameKeyword;
        [SerializeField]
        private ParameterComparer _comparer;

        public override Func<bool> GetResult => () => _comparer.Compare(LhsValue, RhsValue);
        public override string GetCurrentAsString => LhsValue.ToString();
        public override string Name => _nameKeyword;
        public KeywordReference NameKeyword => _nameKeyword;
        public ParameterComparer Comparer => _comparer;


        internal abstract Type ValueType { get; }
        internal virtual object LhsValue { get; set; }
        internal virtual object RhsValue { get; set; }
        internal virtual object LhsDefaultValue { get; set; }
        internal virtual object RhsDefaultValue { get; set; }

        protected Parameter(KeywordReference nameKeyword, ParameterComparer comparer) : base("NULL", null)
        {
            _nameKeyword = nameKeyword;
            _comparer = comparer;
        }

        public virtual Parameter Clone() => MemberwiseClone() as Parameter;

        public abstract ParameterReference GetReference();
    }
    [Serializable]
    public abstract class Parameter<T> : Parameter
    {
        [SerializeField]
        private T _lhsValue;
        [SerializeField]
        private T _rhsValue;


        internal override Type ValueType => typeof(T);
        internal sealed override object LhsValue => _lhsValue;
        internal sealed override object RhsValue => _rhsValue;
        internal sealed override object LhsDefaultValue => DefaultLhsValue();
        internal sealed override object RhsDefaultValue => DefaultRhsValue();

        protected Parameter() : base(new KeywordReference("Null"), null)
        {
            _lhsValue = DefaultLhsValue();
            _rhsValue = DefaultRhsValue();
        }
        protected Parameter(KeywordReference nameKeyword, ParameterComparer comparer) : base(nameKeyword, comparer)
        {
            _lhsValue = DefaultLhsValue();
            _rhsValue = DefaultRhsValue();
        }
        protected Parameter(KeywordReference nameKeyword, ParameterComparer comparer, T lhsValue, T rhsValue) : base(nameKeyword, comparer)
        {
            _lhsValue = lhsValue;
            _rhsValue = rhsValue;
        }

        protected T GetValue() => _lhsValue;
        protected void SetValue(T value) => _lhsValue = value;
        protected virtual T DefaultLhsValue() => default;
        protected virtual T DefaultRhsValue() => default;
    }
}