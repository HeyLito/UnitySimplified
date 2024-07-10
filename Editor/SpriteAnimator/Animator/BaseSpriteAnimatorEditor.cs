#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnitySimplified.SpriteAnimator;
using AnimationState = UnitySimplified.SpriteAnimator.AnimationState;

namespace UnitySimplifiedEditor.SpriteAnimator
{
    [CustomEditor(typeof(BaseSpriteAnimator), true)]
    public class BaseSpriteAnimatorEditor : Editor
    {
        private int? _frameIndex;
        private AnimationState _state;
        private GUIStyle _labelStyle;
        private GUIStyle _foldoutStyle;
        private readonly Dictionary<string, bool> _propertiesExpandedByIDs = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DisplayAnimatorInfoBox();
        }

        public void DisplayAnimatorInfoBox()
        {
            _labelStyle ??= new GUIStyle(EditorStyles.label) { richText = true };
            _foldoutStyle ??= new GUIStyle(EditorStyles.foldout) { richText = true };
            var animator = (target as BaseSpriteAnimator);
            if (animator == null)
                return;

            if (_state != animator.Current.state || _frameIndex != animator.Current.frame)
                Repaint();
            _state = animator.Current.state;
            _frameIndex = animator.Current.frame;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Playing Animation: {(_state != null ? $"<b><i>{_state.Identifier}</i></b>" : "NULL")}", _labelStyle);
            EditorGUILayout.LabelField($"AnimationFrame: {_frameIndex}");
            var statesCount = animator.AnimationStates.Count;
            if (statesCount == 0)
                EditorGUILayout.LabelField($"AnimationStates (Count: {statesCount})", _labelStyle);
            else
            {
                if (!_propertiesExpandedByIDs.TryGetValue(animator.name, out var statesAreExpanded))
                    _propertiesExpandedByIDs[animator.name] = statesAreExpanded = false;
                _propertiesExpandedByIDs[animator.name] = statesAreExpanded = EditorGUILayout.Foldout(statesAreExpanded, $"AnimationStates (Count: {statesCount})", true);
                
                if (statesAreExpanded)
                {
                    EditorGUI.indentLevel++;
                    foreach (var animationState in animator.AnimationStates)
                    {
                        var transitionsCount = animationState.Transitions.Count;
                        if (transitionsCount == 0)
                            EditorGUILayout.LabelField($"<b><i>{animationState.Identifier}</i></b> (TransitionCount: {transitionsCount})", _labelStyle);
                        else
                        {
                            if (!_propertiesExpandedByIDs.TryGetValue(animationState.Identifier, out var stateIsExpanded))
                                _propertiesExpandedByIDs[animationState.Identifier] = stateIsExpanded = false;
                            _propertiesExpandedByIDs[animationState.Identifier] = stateIsExpanded = EditorGUILayout.Foldout(stateIsExpanded, $"<b><i>{animationState.Identifier}</i></b> (TransitionCount: {transitionsCount})", true, _foldoutStyle);
                            
                            if (stateIsExpanded)
                            {
                                EditorGUI.indentLevel++;
                                foreach (var transition in animationState.Transitions)
                                {
                                    var conditionsCount = transition.Conditions.Count;
                                    if (conditionsCount == 0)
                                        EditorGUILayout.LabelField($"ˡ→ <b><i>{transition.OutState.Identifier}</i></b> (ConditionCount: {conditionsCount})", _labelStyle);
                                    else
                                    {
                                        var transitionCacheId = $"{transition.GetHashCode()}+{transition.InState.Identifier}+{transition.OutState.Identifier}";
                                        if (!_propertiesExpandedByIDs.TryGetValue(transitionCacheId, out bool transitionIsExpanded))
                                            _propertiesExpandedByIDs[transitionCacheId] = transitionIsExpanded = false;
                                        _propertiesExpandedByIDs[transitionCacheId] = transitionIsExpanded = EditorGUILayout.Foldout(transitionIsExpanded, $"ˡ→ <b><i>{transition.OutState.Identifier}</i></b> (ConditionCount: {conditionsCount})", true, _foldoutStyle);

                                        if (transitionIsExpanded)
                                        {
                                            EditorGUI.indentLevel++;
                                            foreach (var condition in transition.Conditions)
                                                EditorGUILayout.LabelField($"{condition.Name} (Result: {condition.GetResult()}, Value: {condition.GetCurrentAsString})");
                                            EditorGUI.indentLevel--;
                                        }
                                    }
                                }
                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }
    }
}

#endif