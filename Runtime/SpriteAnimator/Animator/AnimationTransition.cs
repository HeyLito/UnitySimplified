using System;
using System.Collections.Generic;

namespace UnitySimplified.SpriteAnimator
{
    public class AnimationTransition
    {
        public enum FixedEntry { None, Percent, Seconds }

        private readonly HashSet<AnimationCondition> _conditions = new();

        public BaseSpriteAnimator Animator { get; }
        public AnimationState InState { get; }
        public AnimationState OutState { get; }
        public FixedEntry FixedEntryType { get; set; } = default;
        public float FixedEntryDuration { get; set; } = 0;
        public int TransitionOffset { get; set; } = 0;
        public IReadOnlyCollection<AnimationCondition> Conditions => _conditions;

        public AnimationTransition(AnimationState inState, AnimationState outState)
        {
            if (inState.Animator == null)
                throw new NullReferenceException(nameof(inState));
            if (outState.Animator == null)
                throw new NullReferenceException(nameof(outState));
            if (inState.Animator != outState.Animator)
                throw new InvalidOperationException($"{inState}.{nameof(inState.Animator)} does not equal {outState.Animator}.{outState.Animator}");

            if (inState.Equals(null))
                throw new ArgumentNullException(nameof(inState));
            if (outState.Equals(null))
                throw new ArgumentNullException(nameof(outState));

            Animator = inState.Animator;
            InState = inState;
            OutState = outState;
        }

        public bool AddCondition(AnimationCondition condition) => DoAddCondition(condition);
        public bool RemoveCondition(AnimationCondition condition) => DoRemoveCondition(condition);
        public bool TryTransition(float elapsedTime) => DoTryTransition(elapsedTime);
        public void OnSuccessfulTransition() => DoOnSuccessfulTransition();

        private bool DoTryTransition(float elapsedTime)
        {
            if (IsYieldingForEntry(elapsedTime))
                return false;

            if (_conditions.Count == 0)
                return true;

            foreach (var condition in _conditions)
                if (condition.GetResult == null || !condition.GetResult.Invoke())
                    return false;

            foreach (var condition in _conditions)
                condition.OnSuccessfulResult(this);
            return true;
        }
        private bool IsYieldingForEntry(float elapsedTime)
        {
            switch (FixedEntryType)
            {
                default:
                case FixedEntry.None:
                    return false;

                case FixedEntry.Percent:
                    var fps = InState.Animation?.Fps ?? 0;
                    var length = InState.Animation?.Frames?.Length ?? 0;
                    if (fps == 0 || length == 0)
                        return false;
                    var targetTime = 1f / fps * length;
                    return elapsedTime / targetTime < FixedEntryDuration;

                case FixedEntry.Seconds:
                    return elapsedTime < FixedEntryDuration;
            }
        }
        private void DoOnSuccessfulTransition()
        {
            foreach (var condition in _conditions)
                condition.OnSuccessfulResult(this);
        }
        private bool DoAddCondition(AnimationCondition condition)
        {
            if (!_conditions.Add(condition))
                return false;

            InState.Animator.onAnyConditionAdded?.Invoke(condition);
            return true;
        }
        private bool DoRemoveCondition(AnimationCondition condition)
        {
            if (!_conditions.Remove(condition))
                return false;

            InState.Animator.onAnyConditionRemoved?.Invoke(condition);
            return true;
        }
    }
}