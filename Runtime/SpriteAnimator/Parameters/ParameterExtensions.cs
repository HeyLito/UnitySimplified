using System;
using System.Collections.Generic;

namespace UnitySimplified.SpriteAnimator.Parameters
{
    public static class ParameterExtensions
    {
        [NonSerialized]
        private static readonly Dictionary<AbstractSpriteAnimator, Dictionary<Type, Dictionary<string, HashSet<Parameter>>>> CachedAnimatorParameters = new();
        [NonSerialized]
        private static readonly List<(Parameter parameter, AnimationTransition transitionOfParameter)> TempGetAllResults = new();
        [NonSerialized]
        private static readonly HashSet<Parameter> TempNonGenericResults = new();

        public static void GetAllParameters(this AbstractSpriteAnimator animator, List<(Parameter parameter, AnimationTransition transitionOfParameter)> results) => DoGetAllParameters(animator, results);
        public static bool TryGetParameters<T>(this AbstractSpriteAnimator animator, string name, HashSet<Parameter<T>> results) => DoTryGetParameters(animator, name, results);
        public static bool TryGetParameters(this AbstractSpriteAnimator animator, Type valuetype, string name, HashSet<Parameter> results) => DoTryGetParameters(animator, valuetype, name, results);

        private static void DoGetAllParameters(this AbstractSpriteAnimator animator, List<(Parameter, AnimationTransition)> results)
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            foreach (var animationState in animator.AnimationStates)
                foreach (var animationTransition in animationState.Transitions)
                    foreach (var animationCondition in animationTransition.Conditions)
                        if (animationCondition is Parameter parameter)
                            results.Add((parameter, animationTransition));
        }
        private static bool DoTryGetParameters<T>(AbstractSpriteAnimator animator, string name, HashSet<Parameter<T>> results)
        {
            TempNonGenericResults.Clear();
            DoTryGetParameters(animator, typeof(T), name, TempNonGenericResults);
            foreach (var result in TempNonGenericResults)
                if (result is Parameter<T> parameter)
                    results.Add(parameter);
            return results.Count > 0;
        }
        private static bool DoTryGetParameters(AbstractSpriteAnimator animator, Type type, string name, HashSet<Parameter> results)
        {
            if (!CachedAnimatorParameters.TryGetValue(animator, out var cachedParametersByTypes))
            {
                CachedAnimatorParameters[animator] = cachedParametersByTypes = new Dictionary<Type, Dictionary<string, HashSet<Parameter>>>();
                animator.onAnyAnimationConditionAddedCallback += condition => OnAnyConditionAdded(animator, condition);
                animator.onAnyAnimationConditionRemovedCallback += condition => OnAnyConditionRemoved(animator, condition);

                TempGetAllResults.Clear();
                GetAllParameters(animator, TempGetAllResults);
                for (int i = 0; i < TempGetAllResults.Count; i++)
                {
                    var parameter = TempGetAllResults[i].parameter;
                    if (parameter.ValueType != null)
                        DoStoreParameter(parameter, parameter.ValueType, parameter.Name, cachedParametersByTypes);
                }
            }

            if (cachedParametersByTypes.TryGetValue(type, out var cachedParametersByNames))
                if (cachedParametersByNames.TryGetValue(name, out var cachedParameters))
                    foreach (var cachedParameter in cachedParameters)
                        if (!results.Contains(cachedParameter))
                            results.Add(cachedParameter);
            return results.Count > 0;
        }
        private static void DoStoreParameter(Parameter parameter, Type parameterValueType, string parameterName, IDictionary<Type, Dictionary<string, HashSet<Parameter>>> cachedParametersByTypes)
        {
            if (!cachedParametersByTypes.TryGetValue(parameterValueType, out var cachedParametersByNames))
                cachedParametersByTypes[parameterValueType] = cachedParametersByNames = new Dictionary<string, HashSet<Parameter>>();

            if (!cachedParametersByNames.TryGetValue(parameterName, out var parameters))
                cachedParametersByNames[parameterName] = parameters = new HashSet<Parameter>();

            parameters.Add(parameter);
        }
        private static void DoDiscardParameter(Parameter parameter, Type parameterValueType, string parameterName, IDictionary<Type, Dictionary<string, HashSet<Parameter>>> cachedParametersByTypes)
        {
            if (!cachedParametersByTypes.TryGetValue(parameterValueType, out var cachedParametersByNames))
                return;

            if (!cachedParametersByNames.TryGetValue(parameterName, out var parameters))
                return;

            if (parameters.Contains(parameter))
                parameters.Remove(parameter);
        }
        private static void OnAnyConditionAdded(AbstractSpriteAnimator animator, AnimationCondition condition)
        {
            if (condition is not Parameter parameter)
                return;
            if (parameter.ValueType == null)
                return;
            if (CachedAnimatorParameters.TryGetValue(animator, out var cachedParametersByTypes))
                DoStoreParameter(parameter, parameter.ValueType, parameter.Name, cachedParametersByTypes);
        }
        private static void OnAnyConditionRemoved(AbstractSpriteAnimator animator, AnimationCondition condition)
        {
            if (condition is not Parameter parameter)
                return;
            if (parameter.ValueType == null)
                return;
            if (CachedAnimatorParameters.TryGetValue(animator, out var cachedParametersByTypes))
                DoDiscardParameter(parameter, parameter.ValueType, parameter.Name, cachedParametersByTypes);
        }
    }
}