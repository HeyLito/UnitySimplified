using System;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class BoolParameter : Parameter<bool>
    {
        [Serializable]
        private class Reference : ParameterReference<BoolParameter, bool> { }

        public BoolParameter(KeywordReference name) : base(name, new BoolParameterComparer()) { }
        public BoolParameter(KeywordReference name, bool lhsValue, bool rhsValue) : base(name, new BoolParameterComparer(), lhsValue, rhsValue) { }



        public override ParameterReference GetReference() => new Reference();
        public new bool GetValue() => base.GetValue();
        public new void SetValue(bool value) => base.SetValue(value);
        protected override bool DefaultLhsValue() => false;
        protected override bool DefaultRhsValue() => false;
    }
}