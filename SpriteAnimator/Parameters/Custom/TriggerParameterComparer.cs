using System;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class TriggerComparer : ParameterComparer
    {
        public override bool Compare(object lhs, object rhs)
        {
            if (lhs is not Trigger lhsValue)
                throw new NullReferenceException(nameof(lhs));
            if (rhs is not Trigger rhsValue)
                throw new NullReferenceException(nameof(rhs));
            return base.Compare(lhsValue, rhsValue);
        }
    }
}