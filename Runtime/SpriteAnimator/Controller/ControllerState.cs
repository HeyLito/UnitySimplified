using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.VariableReferences;
using static UnitySimplified.SpriteAnimator.AnimationState;
// ReSharper disable NotAccessedField.Local
#pragma warning disable IDE0052

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

        [SerializeField]
        [FormerlySerializedAs("_name")]
        private string name;
        [SerializeField]
        [FormerlySerializedAs("_identifier")]
        private string identifier;
        [SerializeField]
        [FormerlySerializedAs("_motion")]
        private MotionReference motion;
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_isGlobal")]
        private bool isGlobal;
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_isReadOnly")]
        private bool isReadOnly;
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_transitionPorts")]
        private TransitionPort transitionPorts = (TransitionPort)(-1);
        [SerializeField]
        [FormerlySerializedAs("_interruptionSource")]
        private InterruptionSource interruptionSource = InterruptionSource.AnyState;

        public ControllerState() { }
        public ControllerState(SpriteAnimatorController controller, string globalName, TransitionPort transitionPorts) : this(controller, globalName, true, true, transitionPorts) { }
        public ControllerState(SpriteAnimatorController controller, string name, bool isReadOnly = false) : this(controller, name, isReadOnly, false, (TransitionPort)(-1)) { }
        public ControllerState(SpriteAnimatorController controller, string name, bool isReadOnly, bool isGlobal, TransitionPort transitionPorts)
        {
            motion = new MotionReference(null);
            identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            this.name = name;
            this.isReadOnly = isReadOnly;
            this.isGlobal = isGlobal;
            this.transitionPorts = transitionPorts;
        }

        public string Name => name;

        public string GetIdentifier() => identifier;
        internal bool CanConnectTo(ControllerState toState) => transitionPorts.HasFlag(TransitionPort.CanConnectFrom) && toState.transitionPorts.HasFlag(TransitionPort.CanConnectTo);
        internal bool TryGetAsAnimationState(AbstractSpriteAnimator animator, bool createIfMissing, out AnimationState animationState)
        {
            if (isGlobal)
            {
                bool result = animator.TryGetAnimationState(name, out animationState);
                if (!result)
                    Debug.LogWarning($"Controller state \"<b>{name}</b>\" is set as <b>Global</b>, but could not find global animation state on animator?");
                return result;
            }
            else
            {
                if (!animator.TryGetAnimationState(name, out animationState) && createIfMissing)
                    animationState = animator.CreateAnimationState(name, motion, interruptionSource);
                return animationState != null;
            }
        }
    }
}