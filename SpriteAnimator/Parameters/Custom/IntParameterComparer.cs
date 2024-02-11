using System;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class IntParameterComparer : ParameterComparer
    {
        public override bool Compare(object lhsValue, object rhsValue)
        {
            if (lhsValue is not int)
                throw new NullReferenceException(nameof(lhsValue));
            if (rhsValue is not int)
                throw new NullReferenceException(nameof(rhsValue));

            return base.Compare(lhsValue, rhsValue);
        }
    }
}