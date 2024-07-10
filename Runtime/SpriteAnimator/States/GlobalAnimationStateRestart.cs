namespace UnitySimplified.SpriteAnimator
{
    public class GlobalAnimationStateRestart : GlobalAnimationState
    {
        public override string Identifier => "Restart";
        public override TransitionPort TransitionPorts => TransitionPort.CanConnectTo;

        public override void OnSequenceEntered(AnimationState current)
        {
            if (current != this)
                return;
            Animator.Stop();
            Animator.Play();
        }
    }
}