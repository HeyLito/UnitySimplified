using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class ParameterComparer
    {
        [SerializeField]
        [FormerlySerializedAs("_selection")]
        private int selection;

        public virtual string[] Options => new[]
        {
                "==",
                "!=",
            };
        public virtual int Selection
        {
            get => selection;
            set => selection = value >= 0 && value < Options.Length ? value : -1;
        }

        public virtual bool Compare(object lhs, object rhs)
        {
            return Selection switch
            {
                0 => lhs.Equals(rhs),
                1 => !lhs.Equals(rhs),
                _ => throw new NotSupportedException(),
            };
        }

        public ParameterComparer Copy()
        {
            var comparer = new ParameterComparer();
            comparer.selection = selection;
            return comparer;
        }
    }
}