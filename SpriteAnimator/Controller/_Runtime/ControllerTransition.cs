using System;
using System.Collections.Generic;
using UnityEngine;
using static UnitySimplified.SpriteAnimator.AnimationTransition;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [Serializable]
    public class ControllerTransition : IControllerIdentifiable
    {
        [SerializeField, HideInInspector]
        private string _identifier;
        [SerializeField]
        private string _inIdentifier;
        [SerializeField]
        private string _outIdentifier;
        
        [SerializeField]
        private FixedEntry _fixedEntryType;
        [SerializeField]
        private float _fixedEntryDuration;
        [SerializeField]
        private int _transitionOffset;
        
        [SerializeField, SerializeReference]
        private List<ControllerCondition> _conditions;


        internal ControllerTransition(SpriteAnimatorController controller, ControllerState inState, ControllerState outState)
        {
            if (controller == null)
                throw new ArgumentNullException($"{nameof(controller)}");
            if (inState == null)
                throw new ArgumentNullException($"{nameof(inState)}");
            if (outState == null)
                throw new ArgumentNullException($"{nameof(outState)}");

            _fixedEntryType = FixedEntry.Percent;
            _fixedEntryDuration = 1;
            _transitionOffset = 0;
            _identifier = IControllerIdentifiable.GenerateLocalUniqueIdentifier(controller.ExistingIdentifiers());
            _inIdentifier = inState.GetIdentifier();
            _outIdentifier = outState.GetIdentifier();
            _conditions = new List<ControllerCondition>();
        }



        public string GetIdentifier() => _identifier;
        public string GetInIdentifier() => _inIdentifier;
        public string GetOutIdentifier() => _outIdentifier;
        public bool Validate(SpriteAnimatorController controller) => 
            !string.IsNullOrEmpty(_identifier) &&
            !string.IsNullOrEmpty(_inIdentifier) &&
            !string.IsNullOrEmpty(_outIdentifier) &&
            controller.ContainsState(_inIdentifier) &&
            controller.ContainsState(_outIdentifier);
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
                transition.FixedEntryType = _fixedEntryType;
                transition.FixedEntryDuration = _fixedEntryDuration;
                transition.TransitionOffset = _transitionOffset;
                foreach (var condition in _conditions)
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