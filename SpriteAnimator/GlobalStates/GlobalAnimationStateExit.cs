namespace UnitySimplified.SpriteAnimator
{
    public class GlobalAnimationStateExit : GlobalAnimationState
    {
        public override string Identifier => "Exit";
        public override TransitionPort TransitionPorts => TransitionPort.CanConnectTo;
    }
}