using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnitySimplified.Collections;

namespace UnitySimplified.SpriteAnimator.Controller
{
    [CreateAssetMenu(fileName = "New SpriteAnimatorController", menuName = "Unity Simplified/Sprite Animator Controller")]
    public sealed class SpriteAnimatorController : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public class ControllerParameterList : ListWrapper<ControllerParameter> { }
        [Serializable]
        public class ControllerStateList : ListWrapper<ControllerState> { }
        [Serializable]
        public class ControllerTransitionList : ListWrapper<ControllerTransition> { }

        [SerializeField]
        [FormerlySerializedAs("_parameters")]
        private ControllerParameterList parameters = new();
        [SerializeField]
        [FormerlySerializedAs("_states")]
        private ControllerStateList states = new();
        [SerializeField]
        [FormerlySerializedAs("_transitions")]
        private ControllerTransitionList transitions = new();

        [NonSerialized]
        private readonly HashSet<AbstractSpriteAnimator> _activeAnimators = new();
        [NonSerialized]
        private readonly HashSet<string> _existingIdentifiers = new();
        [NonSerialized]
        private readonly Dictionary<string, IControllerIdentifiable> _identifiablesByIdentifiers = new ();
        [NonSerialized]
        private readonly Dictionary<string, ControllerState> _statesByNames = new();
        [NonSerialized]
        private readonly Dictionary<string, GlobalAnimationState> _globalAnimationStatesByNames = new();

        public IReadOnlyCollection<ControllerState> States => _statesByNames.Values;
        internal ControllerParameterList InternalParameters => parameters;
        internal ControllerStateList InternalStates => states;
        internal ControllerTransitionList InternalTransitions => transitions;

        public void AttachToAnimator(AbstractSpriteAnimator animator)
        {
            if (Application.IsPlaying(animator))
            {
                if (_activeAnimators.Add(animator))
                {
                    foreach (var transition in transitions)
                        if (TryGetStateFromIdentifier(transition.GetInIdentifier(), out var inControllerState) && TryGetStateFromIdentifier(transition.GetOutIdentifier(), out var outControllerState))
                            if (inControllerState.TryGetAsAnimationState(animator, true, out var inAnimationState) && outControllerState.TryGetAsAnimationState(animator, true, out var outAnimationState))
                                transition.AddToAnimationState(this, inAnimationState, outAnimationState, out _);
                }
                else Debug.LogWarning($"Could not attach {this} to {animator} because it already contains it.");
            }
            else throw new InvalidOperationException($"Should only be attaching {typeof(SpriteAnimatorController)} within runtime.");
        }
        public void DetachFromAnimator(AbstractSpriteAnimator animator)
        {
            if (Application.IsPlaying(animator))
            {
                if (_activeAnimators.Remove(animator))
                {
                    foreach (var transition in transitions)
                        if (TryGetStateFromIdentifier(transition.GetInIdentifier(), out var inControllerState) && TryGetStateFromIdentifier(transition.GetOutIdentifier(), out var outControllerState))
                            if (inControllerState.TryGetAsAnimationState(animator, true, out var inAnimatorState) && outControllerState.TryGetAsAnimationState(animator, true, out var outAnimatorState))
                                transition.RemoveFromAnimationState(this, inAnimatorState, outAnimatorState);
                }
                else Debug.LogWarning($"Could not detach {this} from {animator} because it does not contain it.");
            }
            else throw new InvalidOperationException($"Should only be detaching {typeof(SpriteAnimatorController)} within runtime.");
        }

        internal ISet<string> ExistingIdentifiers() => _existingIdentifiers;

        public bool TryGetStateFromName(string stateName, out ControllerState state) => DoGetControllerStateFromName(stateName, out state);
        public bool TryGetStateFromIdentifier(string identifier, out ControllerState state) => DoGetIdentifiableFromIdentifier(identifier, out state);
        public bool TryGetParameterFromIdentifier(string identifier, out ControllerParameter parameter) => DoGetIdentifiableFromIdentifier(identifier, out parameter);

        private bool DoGetControllerStateFromName(string stateName, out ControllerState state)
        {
            if (states.Count != _statesByNames.Count)
                ((ISerializationCallbackReceiver)this).OnAfterDeserialize();
            return _statesByNames.TryGetValue(stateName, out state);
        }
        private bool DoGetIdentifiableFromIdentifier<T>(string identifier, out T identifiable) where T : IControllerIdentifiable
        {
            identifiable = default;
            
            if (string.IsNullOrEmpty(identifier))
                return false;
            if (!_identifiablesByIdentifiers.TryGetValue(identifier, out var result))
                return false;
            if (result is not T t)
                return false;
            
            identifiable = t;
            return true;
        }

        public bool ContainsState(string identifier) => DoContainsState(identifier);

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            for (int i = transitions.Count - 1; i >= 0; i--)
            {
                string transitionAsMessage = $"Indexed <b>ControllerTransition</b>(<color=yellow>{transitions[i].GetIdentifier()}</color>) at [{i}]";
                if (!_identifiablesByIdentifiers.TryGetValue(transitions[i].GetIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} is not contained by the controller.");
                    continue;
                }
                if (!TryGetStateFromIdentifier(transitions[i].GetInIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} can not locate <b>ControllerState</b>(<color=yellow>{transitions[i].GetInIdentifier()}</color>).");
                    continue;
                }
                if (!TryGetStateFromIdentifier(transitions[i].GetOutIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} can not locate <b>ControllerState</b>(<color=yellow>{transitions[i].GetOutIdentifier()}</color>).");
                    continue;
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _globalAnimationStatesByNames.Clear();
            _existingIdentifiers.Clear();
            _identifiablesByIdentifiers.Clear();
            _statesByNames.Clear();

            foreach (var globalState in GlobalAnimationState.States)
                _globalAnimationStatesByNames.Add(globalState.Identifier, globalState);

            for (int i = 0; i < parameters.Count; i++)
                if (parameters[i] != null && !string.IsNullOrEmpty(parameters[i].GetIdentifier()))
                {
                    _existingIdentifiers.Add(parameters[i].GetIdentifier());
                    _identifiablesByIdentifiers[parameters[i].GetIdentifier()] = parameters[i];
                }
                else parameters.RemoveAt(i--);

            for (int i = 0; i < states.Count; i++)
                if (states[i] != null && !string.IsNullOrEmpty(states[i].GetIdentifier()))
                {
                    _existingIdentifiers.Add(states[i].GetIdentifier());
                    _identifiablesByIdentifiers[states[i].GetIdentifier()] = states[i];
                    _statesByNames[states[i].Name] = states[i];
                }
                else states.RemoveAt(i--);

            for (int i = 0; i < transitions.Count; i++)
                if (transitions[i] != null && !string.IsNullOrEmpty(transitions[i].GetIdentifier()))
                {
                    _existingIdentifiers.Add(transitions[i].GetIdentifier());
                    _identifiablesByIdentifiers[transitions[i].GetIdentifier()] = transitions[i];
                }
                else
                {
                    //controllerTransitions.RemoveValueAt(i--);
                    Debug.Log($"{transitions[i]}");
                }


            foreach (var globalState in _globalAnimationStatesByNames.Values)
                if (!_statesByNames.ContainsKey(globalState.Identifier))
                {
                    var controllerState = new ControllerState(this, globalState.Identifier, globalState.TransitionPorts);
                    states.Add(controllerState);
                    _existingIdentifiers.Add(globalState.Identifier);
                    _statesByNames[controllerState.Name] = controllerState;
                    _identifiablesByIdentifiers[controllerState.GetIdentifier()] = controllerState;
                }
        }

        private bool DoContainsState(string identifier) => _identifiablesByIdentifiers.TryGetValue(identifier, out var identifiable) && identifiable is ControllerState;
    }
}