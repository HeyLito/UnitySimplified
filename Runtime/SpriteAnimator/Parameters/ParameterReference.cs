using System;
using UnityEngine;
using UnityEngine.Serialization;

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
        [FormerlySerializedAs("_value")]
        private TValue value;

        public override Type Type => typeof(TParameter);
        public override Type ValueType => typeof(TValue);
        public override object Value { get => value; internal set => this.value = (TValue)value; }
    }
}