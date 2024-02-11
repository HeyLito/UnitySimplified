using System;
using System.Collections.Generic;
using UnitySimplified.Collections;

namespace UnitySimplified.SpriteAnimator
{
    public class AnimationState
    {
        [Flags]
        public enum TransitionPort
        {
            None = 0,
            CanConnectFrom = 1 << 1,
            CanConnectTo = 1 << 2,
        }

        private readonly OrderedSet<AnimationTransition> _transitions = new();

        internal virtual bool IsGlobal => false;
        public virtual int Priority { get; private set; } = 0;
        public virtual string Identifier { get; private set; } = "";
        public virtual InterruptionSource InterruptionSource { get; private set; }
        public virtual TransitionPort TransitionPorts => TransitionPort.CanConnectFrom | TransitionPort.CanConnectTo;

        public BaseSpriteAnimator Animator { get; private set; }
        public SpriteAnimation Animation { get; private set; }
        public ICollection<AnimationTransition> Transitions => _transitions;


        internal static AnimationState CreateAnimationState(Type type, string identifier, BaseSpriteAnimator animator, SpriteAnimation animation, InterruptionSource interruptionSource)
        {
            string baseExceptionMessage = "Could not create animation state;";
            if (string.IsNullOrEmpty(identifier))
                throw new ArgumentException($"{baseExceptionMessage} Method parameter {nameof(identifier)} is empty or null.");
            if (animator == null)
                throw new ArgumentNullException($"{baseExceptionMessage} Method parameter {nameof(animator)} is null.");
            if (typeof(AnimationState).IsAssignableFrom(type))
            {
                var state = (AnimationState)Activator.CreateInstance(type);
                if (state == null)
                {
                    throw new ArgumentNullException();
                }
                else
                {
                    state.Identifier = identifier;
                    state.Animator = animator;
                    state.Animation = animation;
                    state.InterruptionSource = interruptionSource;
                }

                return state;
            }
            else throw new InvalidOperationException($"{baseExceptionMessage} Type {type} is not an {typeof(AnimationState)} nor a subclass of it");
        }



        public bool TryGetNext(float time, out AnimationTransition next) => DoTryGetNext(time, out next);
        public bool AddTransition(AnimationState to, out AnimationTransition transition) => DoAddTransition(to, out transition);
        public bool RemoveTransition(AnimationTransition transition) => DoRemoveTransition(transition);
        public virtual void OnSequenceEntered(AnimationState current) { }
        public virtual void OnSequenceExited(AnimationState current) { }
        public virtual void OnSequenceUpdate(AnimationState current, float time) { }

        private bool DoValidate(out string exceptionMessage)
        {
            exceptionMessage = null;
            if (Animator == null)
                exceptionMessage = "Animation state does not contain reference to animator.";
            else if (string.IsNullOrEmpty(Identifier))
                exceptionMessage = "Identifier of animation state is null or empty.";
            else if (!Animator.TryGetAnimationState(Identifier, out AnimationState animationState) || animationState != this)
                exceptionMessage = "Animator does not contain animation state.";
            return exceptionMessage == null;
        }
        private bool DoTryGetNext(float elapsedTime, out AnimationTransition next)
        {
            foreach (var transition in _transitions)
            {
                if (!transition.InState.DoValidate(out var exceptionMessage))
                    throw new InvalidOperationException(exceptionMessage);
                if (!transition.OutState.DoValidate(out exceptionMessage))
                    throw new InvalidOperationException(exceptionMessage);

                if (transition.TryTransition(elapsedTime))
                {
                    switch (Animator.Current.state.InterruptionSource)
                    {
                        default:
                        case InterruptionSource.None:
                            if (Animator.Current.state == this || Animator.Current.state == null)
                            {
                                next = transition;
                                transition.OnSuccessfulTransition();
                                return true;
                            }
                            break;
                        case InterruptionSource.AnyState:
                            next = transition;
                            transition.OnSuccessfulTransition();
                            return true;
                        case InterruptionSource.NextState:
                            if (Animator.Current.state != transition.OutState)
                            {
                                next = transition;
                                transition.OnSuccessfulTransition();
                                return true;
                            }
                            break;
                        case InterruptionSource.CurrentState:
                            if (Animator.Current.state == transition.OutState)
                            {
                                next = transition;
                                transition.OnSuccessfulTransition();
                                return true;
                            }
                            break;
                    }
                }
            }
            next = default;
            return false;
        }
        private bool DoAddTransition(AnimationState to, out AnimationTransition transition)
        {
            if (!DoValidate(out var exceptionMessage))
                throw new InvalidOperationException(exceptionMessage);

            transition = null;

            if (!TransitionPorts.HasFlag(TransitionPort.CanConnectFrom))
                return false;
            if (to != null)
            {
                if (!to.TransitionPorts.HasFlag(TransitionPort.CanConnectTo))
                    return false;

                _transitions.Add(transition = new AnimationTransition(this, to));
                foreach (var condition in transition.Conditions)
                    Animator.onAnyConditionAdded?.Invoke(condition);
                return true;
            }
            else throw new ArgumentNullException(nameof(to));
        }
        private bool DoRemoveTransition(AnimationTransition transition)
        {
            if (!DoValidate(out string exceptionMessage))
                throw new InvalidOperationException(exceptionMessage);

            if (transition != null)
            {
                if (_transitions.Contains(transition))
                {
                    _transitions.Remove(transition);
                    foreach (var condition in transition.Conditions)
                        Animator.onAnyConditionRemoved?.Invoke(condition);
                    return true;
                }
                return false;
            }
            else throw new ArgumentNullException(nameof(transition));
        }
    }
}