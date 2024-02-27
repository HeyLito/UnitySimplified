using System;
using UnityEngine;
using UnitySimplified.VariableReferences;
using static UnitySimplified.SpriteAnimator.AnimationState;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerState : IControllerIdentifiable
    {
        [Serializable]
        private class MotionReference : VariableReference<SpriteAnimation, SpriteAnimationClip>
        {
            public MotionReference(SpriteAnimation value) : base(value) { }
            public static implicit operator SpriteAnimation(MotionReference reference) => reference.Value;
        }

#pragma warning disable IDE0052
        [SerializeField]
        private string _name;
        [SerializeField]
        private string _identifier;
        [SerializeField]
        private MotionReference _motion;
        [SerializeField, HideInInspector]
        private bool _isGlobal;
        [SerializeField, HideInInspector]
        private bool _isReadOnly;
        [SerializeField, HideInInspector]
        private TransitionPort _transitionPorts = (TransitionPort)(-1);
        [SerializeField]
        private InterruptionSource _interruptionSource = InterruptionSource.AnyState;
#pragma warning restore IDE0052

        public string Name => _name;

        public ControllerState() { }
        public ControllerState(SpriteAnimatorController controller, string globalName, TransitionPort transitionPorts)
        {
            _motion = new MotionReference(null);
            _name = globalName;
            _identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            _isGlobal = true;
            _isReadOnly = true;
            _transitionPorts = transitionPorts;
        }
        public ControllerState(SpriteAnimatorController controller, string name, bool isReadOnly = false)
        {
            _motion = new MotionReference(null);
            _name = name;
            _identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            _isGlobal = false;
            _isReadOnly = isReadOnly;
            _transitionPorts = (TransitionPort)(-1);
        }



        public string GetIdentifier() => _identifier;
        internal bool CanConnectTo(ControllerState toState) => _transitionPorts.HasFlag(TransitionPort.CanConnectFrom) && toState._transitionPorts.HasFlag(TransitionPort.CanConnectTo);
        internal bool TryGetAsAnimationState(BaseSpriteAnimator animator, bool createIfMissing, out AnimationState animationState)
        {
            if (_isGlobal)
            {
                bool result = animator.TryGetAnimationState(_name, out animationState);
                if (!result)
                    Debug.LogWarning($"Controller state \"<b>{_name}</b>\" is set as <b>Global</b>, but could not find global animation state on animator?");
                return result;
            }
            else
            {
                if (!animator.TryGetAnimationState(_name, out animationState) && createIfMissing)
                    animationState = animator.CreateAnimationState(_name, _motion, _interruptionSource);
                return animationState != null;
            }
        }
    }
}