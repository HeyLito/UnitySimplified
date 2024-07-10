using System;
using UnityEngine;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public abstract class ParameterReference
    {
        public abstract Type Type { get; }
        public abstract Type ValueType { get; }
        public abstract object Value { get; internal set; }
    }

    [Serializable]
    public class ParameterReference<TParameter, TValue> : ParameterReference where TParameter : Parameter<TValue>
    {
        [SerializeField]
        private TValue _value;

        public override Type Type => typeof(TParameter);
        public override Type ValueType => typeof(TValue);
        public override object Value { get => _value; internal set => _value = (TValue)value; }
    }
}