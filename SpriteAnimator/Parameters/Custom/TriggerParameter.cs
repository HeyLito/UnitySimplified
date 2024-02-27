using System;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    [Serializable]
    public class TriggerParameter : Parameter<Trigger>
    {
        [Serializable]
        private class Reference : ParameterReference<TriggerParameter, Trigger> { }

        public TriggerParameter(KeywordReference name) : base(name, new TriggerComparer()) { }
        public TriggerParameter(KeywordReference name, bool value) : this(name, value, true) { }
        private TriggerParameter(KeywordReference name, Trigger lhs, Trigger rhs) : base(name, new TriggerComparer(), lhs, rhs) { }



        public override ParameterReference GetReference() => new Reference();
        public virtual void ResetValue() => SetValue(false);
        public virtual void SetValue() => SetValue(true);
        public override void OnSuccessfulResult(AnimationTransition context) => context.Animator.ResetTriggerParameter(NameKeyword);

        protected override Trigger DefaultLhsValue() => false;
        protected override Trigger DefaultRhsValue() => true;
    }
}