using System;
using UnityEngine;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class ParameterComparer
    {
        [SerializeField]
        private int _selection = 0;

        public virtual string[] Options => new[]
        {
                "==",
                "!=",
            };
        public virtual int Selection
        {
            get => _selection;
            set => _selection = value >= 0 && value < Options.Length ? value : -1;
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
            comparer._selection = _selection;
            return comparer;
        }
    }
}
//namespace UnitySimplified.SpriteAnimator
//{
//    [Serializable]
//    public class ParameterComparer
//    {
//        public virtual bool Compare(object lhs, object rhs) => throw new NotImplementedException();

//        public virtual ParameterComparer Copy()
//        {
//            var comparer = new ParameterComparer();
//            return comparer;
//        }
//    }
//}