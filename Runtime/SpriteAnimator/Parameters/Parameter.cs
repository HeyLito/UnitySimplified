using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public abstract class Parameter : AnimationCondition
    {
        [SerializeField]
        [FormerlySerializedAs("_nameKeyword")]
        private KeywordReference nameKeyword;
        [SerializeField]
        [FormerlySerializedAs("_comparer")]
        private ParameterComparer comparer;

        public override Func<bool> GetResult => () => comparer.Compare(LhsValue, RhsValue);
        public override string GetCurrentAsString => LhsValue.ToString();
        public override string Name => nameKeyword;
        public KeywordReference NameKeyword => nameKeyword;
        public ParameterComparer Comparer => comparer;

        protected Parameter(KeywordReference nameKeyword, ParameterComparer comparer) : base("NULL", null)
        {
            this.nameKeyword = nameKeyword;
            this.comparer = comparer;
        }

        internal abstract Type ValueType { get; }
        internal virtual object LhsValue { get; set; }
        internal virtual object RhsValue { get; set; }
        internal virtual object LhsDefaultValue { get; set; }
        internal virtual object RhsDefaultValue { get; set; }

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

        public T GetValue() => _lhsValue;
        public void SetValue(T value) => _lhsValue = value;
        protected virtual T DefaultLhsValue() => default;
        protected virtual T DefaultRhsValue() => default;
    }
}