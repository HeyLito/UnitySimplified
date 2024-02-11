namespace UnitySimplified.SpriteAnimator
{
    public class GlobalAnimationStateAny : GlobalAnimationState
    {
        public override string Identifier => "Any";
        public override TransitionPort TransitionPorts => TransitionPort.CanConnectFrom;

        public override void OnSequenceUpdate(AnimationState current, float time)
        {
            base.OnSequenceUpdate(current, time);
            if (current != this && Transitions.Count > 0)
                if (Animator.Next == (null, null) && TryGetNext(time, out AnimationTransition next))
                    Animator.PlayNext(next.OutState, next.TransitionOffset);
        }
    }
}