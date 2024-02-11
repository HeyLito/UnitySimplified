using System;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class IntParameter : Parameter<int>
    {
        public class Reference : ParameterReference<IntParameter, int> { }

        public IntParameter(KeywordReference name) : base(name, new IntParameterComparer()) { }
        public IntParameter(KeywordReference name, int lhsValue, int rhsValue) : base(name, new IntParameterComparer(), lhsValue, rhsValue) { }

        public override ParameterReference GetReference() => new Reference();
    }
}