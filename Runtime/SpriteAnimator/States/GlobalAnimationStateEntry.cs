namespace UnitySimplified.SpriteAnimator
{
    public class GlobalAnimationStateEntry : GlobalAnimationState
    {
        public override int PrioritySort => -10;
        public override string Identifier => "Entry";
        public override TransitionPort TransitionPorts => TransitionPort.CanConnectFrom;

        public override bool OnTryPlay(AbstractSpriteAnimator animator) => !animator.IsPlaying() && animator.Play(this);
    }
}