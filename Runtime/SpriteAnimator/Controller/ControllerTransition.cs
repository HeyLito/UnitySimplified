using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static UnitySimplified.SpriteAnimator.AnimationTransition;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerTransition : IControllerIdentifiable
    {
        [SerializeField, HideInInspector]
        [FormerlySerializedAs("_identifier")]
        private string identifier;
        [SerializeField]
        [FormerlySerializedAs("_inIdentifier")]
        private string inIdentifier;
        [SerializeField]
        [FormerlySerializedAs("_outIdentifier")]
        private string outIdentifier;
        
        [SerializeField]
        [FormerlySerializedAs("_fixedEntryType")]
        private FixedEntry fixedEntryType;
        [SerializeField]
        [FormerlySerializedAs("_fixedEntryDuration")]
        private float fixedEntryDuration;
        [SerializeField]
        [FormerlySerializedAs("_transitionOffset")]
        private int frameOffset;
        
        [SerializeField, SerializeReference]
        [FormerlySerializedAs("_conditions")]
        private List<ControllerCondition> conditions;

        internal ControllerTransition(SpriteAnimatorController controller, ControllerState inState, ControllerState outState)
        {
            if (controller == null)
                throw new ArgumentNullException($"{nameof(controller)}");
            if (inState == null)
                throw new ArgumentNullException($"{nameof(inState)}");
            if (outState == null)
                throw new ArgumentNullException($"{nameof(outState)}");

            fixedEntryType = FixedEntry.Percent;
            fixedEntryDuration = 1;
            frameOffset = 0;
            identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            inIdentifier = inState.GetIdentifier();
            outIdentifier = outState.GetIdentifier();
            conditions = new List<ControllerCondition>();
        }

        public string GetIdentifier() => identifier;
        public string GetInIdentifier() => inIdentifier;
        public string GetOutIdentifier() => outIdentifier;
        public bool Validate(SpriteAnimatorController controller) => 
            !string.IsNullOrEmpty(identifier) &&
            !string.IsNullOrEmpty(inIdentifier) &&
            !string.IsNullOrEmpty(outIdentifier) &&
            controller.ContainsState(inIdentifier) &&
            controller.ContainsState(outIdentifier);
        public void AddToAnimationState(SpriteAnimatorController controller, AnimationState inState, AnimationState outState, out AnimationTransition transition)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (inState == null)
                throw new ArgumentNullException(nameof(inState));
            if (outState == null)
                throw new ArgumentNullException(nameof(outState));

            if (inState.AddTransition(outState, out transition))
            {
                transition.FixedEntryType = fixedEntryType;
                transition.FixedEntryDuration = fixedEntryDuration;
                transition.FrameOffset = frameOffset;
                foreach (var condition in conditions)
                    if (condition.TryGetCondition(controller, out var animationCondition))
                        transition.AddCondition(animationCondition);
            }
            else Debug.LogError($"Couldn't add transition from <b>{inState.Identifier}</b> to <b>{outState.Identifier}</b>?");
        }
        public bool RemoveFromAnimationState(SpriteAnimatorController controller, AnimationState inState, AnimationState outState)
        {
            if (controller == null)
                throw new ArgumentNullException(nameof(controller));
            if (inState == null)
                throw new ArgumentNullException(nameof(inState));
            if (outState == null)
                throw new ArgumentNullException(nameof(outState));

            AnimationTransition animationTransition = null;
            foreach (var iteratedAnimationTransition in inState.Transitions)
                if (animationTransition.InState == inState && animationTransition.OutState == outState)
                {
                    animationTransition = iteratedAnimationTransition;
                    break;
                }
            if (animationTransition != null)
            {
                inState.RemoveTransition(animationTransition);
                if (inState.Transitions.Count == 0 && inState is not GlobalAnimationState)
                    inState.Animator.RemoveAnimationState(inState);
                if (outState.Transitions.Count == 0 && outState is not GlobalAnimationState)
                    outState.Animator.RemoveAnimationState(outState);
                return true;
            }
            else return false;
        }
    }
}