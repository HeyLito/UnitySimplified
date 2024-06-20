using System;
using UnityEngine;

namespace UnitySimplified.GamePrefs
{
    [Serializable]
    public abstract class BaseLocalGamePref
    {
        public abstract object Value { get; }
        public abstract Type ValueType { get; }
    }
    [Serializable]
    public abstract class LocalGamePref<T> : BaseLocalGamePref
    {
        [SerializeField]
        private T value;

        public override object Value => value;
        public override Type ValueType => typeof(T);
    }
}