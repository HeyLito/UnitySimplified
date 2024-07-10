using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.VariableReferences;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    public static class BoolParameterExtensions
    {
        [NonSerialized]
        private static readonly HashSet<Parameter<bool>> TempBoolResults = new();

        public static bool GetBoolParameter(this AbstractSpriteAnimator animator, string name) => DoGetBoolParameter(animator, name);
        public static bool GetBoolParameter(this AbstractSpriteAnimator animator, KeywordReference nameKeyword) => DoGetBoolParameter(animator, nameKeyword.Value);
        public static void SetBoolParameter(this AbstractSpriteAnimator animator, string name, bool value) => DoSetBoolParameter(animator, name, value);
        public static void SetBoolParameter(this AbstractSpriteAnimator animator, KeywordReference nameKeyword, bool value) => DoSetBoolParameter(animator, nameKeyword.Value, value);

        private static bool DoGetBoolParameter(AbstractSpriteAnimator animator, string name)
        {
            TempBoolResults.Clear();
            if (animator.TryGetParameters(name, TempBoolResults))
            {
                foreach (var result in TempBoolResults)
                {
                    if (result != null && result is BoolParameter boolParameter)
                        return boolParameter.GetValue();
                }
                throw new NullReferenceException("Should had returned a parameter, but did not?");
            }
            else
            {
                Debug.LogWarning($"Does not contain a {nameof(BoolParameter)} named \"{name}\".");
                return false;
            }
        }
        private static void DoSetBoolParameter(this AbstractSpriteAnimator animator, string name, bool value)
        {
            TempBoolResults.Clear();
            if (animator.TryGetParameters(name, TempBoolResults))
            {
                foreach (var result in TempBoolResults)
                    if (result != null && result is BoolParameter boolParameter)
                        boolParameter.SetValue(value);
            }
            else Debug.LogWarning($"Does not contain a {nameof(BoolParameter)} named \"{name}\".");
        }
    }
}