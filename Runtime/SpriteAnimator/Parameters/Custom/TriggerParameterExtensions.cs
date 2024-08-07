using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    public static class TriggerParameterExtensions
    {
        [NonSerialized]
        private static readonly HashSet<Parameter<Trigger>> TriggerResults = new();

        public static void ResetTriggerParameter(this AbstractSpriteAnimator animator, string name) => DoResetTriggerParameter(animator, name);
        public static void ResetTriggerParameter(this AbstractSpriteAnimator animator, KeywordReference nameReference) => DoResetTriggerParameter(animator, nameReference);
        public static void SetTriggerParameter(this AbstractSpriteAnimator animator, string name) => DoSetTriggerParameter(animator, name);
        public static void SetTriggerParameter(this AbstractSpriteAnimator animator, KeywordReference nameKeyword) => DoSetTriggerParameter(animator, nameKeyword.Value);

        private static void DoResetTriggerParameter(this AbstractSpriteAnimator animator, string name)
        {
            TriggerResults.Clear();
            if (animator.TryGetParameters(name, TriggerResults))
            {
                foreach (var result in TriggerResults)
                    if (result != null && result is TriggerParameter triggerParameter)
                        triggerParameter.ResetValue();
            }
            else Debug.LogWarning($"{animator} does not contain a {nameof(TriggerParameter)} named \"{name}\"");
        }
        private static void DoSetTriggerParameter(this AbstractSpriteAnimator animator, string name)
        {
            TriggerResults.Clear();
            if (animator.TryGetParameters(name, TriggerResults))
            {
                foreach (var result in TriggerResults)
                    if (result != null && result is TriggerParameter triggerParameter)
                        triggerParameter.SetValue();
            }
            else Debug.LogWarning($"{animator} Does not contain a {nameof(TriggerParameter)} named \"{name}\"");
        }
    }
}