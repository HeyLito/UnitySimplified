using System;
using System.Collections.Generic;
using UnityEngine;
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
        private ControllerParameterList _parameters = new();
        [SerializeField]
        private ControllerStateList _states = new();
        [SerializeField]
        private ControllerTransitionList _transitions = new();

        [NonSerialized]
        private readonly HashSet<BaseSpriteAnimator> _activeAnimators = new();
        [NonSerialized]
        private readonly HashSet<string> _existingIdentifiers = new();
        [NonSerialized]
        private readonly Dictionary<string, IControllerIdentifiable> _identifiablesByIdentifiers = new ();
        [NonSerialized]
        private readonly Dictionary<string, ControllerState> _statesByNames = new();
        [NonSerialized]
        private readonly Dictionary<string, GlobalAnimationState> _globalAnimationStatesByNames = new();

        public IReadOnlyCollection<ControllerState> States => _statesByNames.Values;

        public void AttachToAnimator(BaseSpriteAnimator animator)
        {
            if (Application.IsPlaying(animator))
            {
                if (_activeAnimators.Add(animator))
                {
                    foreach (var transition in _transitions)
                        if (TryGetStateFromIdentifier(transition.GetInIdentifier(), out var inControllerState) && TryGetStateFromIdentifier(transition.GetOutIdentifier(), out var outControllerState))
                            if (inControllerState.TryGetAsAnimationState(animator, true, out var inAnimationState) && outControllerState.TryGetAsAnimationState(animator, true, out var outAnimationState))
                                transition.AddToAnimationState(this, inAnimationState, outAnimationState, out _);
                }
                else Debug.LogWarning($"Could not attach {this} to {animator} because it already contains it.");
            }
            else throw new InvalidOperationException($"Should only be attaching {typeof(SpriteAnimatorController)} within runtime.");
        }
        public void DetachFromAnimator(BaseSpriteAnimator animator)
        {
            if (Application.IsPlaying(animator))
            {
                if (_activeAnimators.Remove(animator))
                {
                    foreach (var transition in _transitions)
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
            if (_states.Count != _statesByNames.Count)
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

        public bool CreateTransition(ControllerState inControllerState, ControllerState outControllerState, out ControllerTransition controllerTransition)
        {
            if (inControllerState != null && outControllerState != null && inControllerState != outControllerState)
            {
                if (!string.IsNullOrEmpty(inControllerState.GetIdentifier()) && !string.IsNullOrEmpty(outControllerState.GetIdentifier()) && inControllerState.GetIdentifier() != outControllerState.GetIdentifier())
                {
                    if (inControllerState.CanConnectTo(outControllerState))
                    {
                        controllerTransition = new ControllerTransition(this, inControllerState, outControllerState);
                        _transitions.Add(controllerTransition);
                        return true;
                    }
                }
            }
            controllerTransition = null;
            return false;
        }

        public bool ContainsState(string identifier) => DoContainsState(identifier);

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            for (int i = _transitions.Count - 1; i >= 0; i--)
            {
                string transitionAsMessage = $"Indexed <b>ControllerTransition</b>(<color=yellow>{_transitions[i].GetIdentifier()}</color>) at [{i}]";
                if (!_identifiablesByIdentifiers.TryGetValue(_transitions[i].GetIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} is not contained by the controller.");
                    continue;
                }
                if (!TryGetStateFromIdentifier(_transitions[i].GetInIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} can not locate <b>ControllerState</b>(<color=yellow>{_transitions[i].GetInIdentifier()}</color>).");
                    continue;
                }
                if (!TryGetStateFromIdentifier(_transitions[i].GetOutIdentifier(), out _))
                {
                    Debug.LogError($"{transitionAsMessage} can not locate <b>ControllerState</b>(<color=yellow>{_transitions[i].GetOutIdentifier()}</color>).");
                    continue;
                }
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _existingIdentifiers.Clear();
            _identifiablesByIdentifiers.Clear();
            _statesByNames.Clear();
            _globalAnimationStatesByNames.Clear();


            foreach (var globalState in GlobalAnimationState.States)
                _globalAnimationStatesByNames.Add(globalState.Identifier, globalState);


            for (int i = 0; i < _parameters.Count; i++)
                if (_parameters[i] != null && !string.IsNullOrEmpty(_parameters[i].GetIdentifier()))
                    _identifiablesByIdentifiers[_parameters[i].GetIdentifier()] = _parameters[i];
                else _parameters.RemoveAt(i--);


            for (int i = 0; i < _states.Count; i++)
                if (!string.IsNullOrEmpty(_states[i].GetIdentifier()))
                {
                    _identifiablesByIdentifiers[_states[i].GetIdentifier()] = _states[i];
                    _statesByNames[_states[i].Name] = _states[i];
                }
                else _states.RemoveAt(i--);


            for (int i = 0; i < _transitions.Count; i++)
                if (!string.IsNullOrEmpty(_transitions[i].GetIdentifier()))
                    _identifiablesByIdentifiers[_transitions[i].GetIdentifier()] = _transitions[i];
                else
                {
                    //controllerTransitions.RemoveValueAt(i--);
                    Debug.Log($"{_transitions[i]}");
                }


            foreach (var globalState in _globalAnimationStatesByNames.Values)
                if (!_statesByNames.ContainsKey(globalState.Identifier))
                {
                    var controllerState = new ControllerState(this, globalState.Identifier, globalState.TransitionPorts);
                    _states.Add(controllerState);
                    _statesByNames[controllerState.Name] = controllerState;
                    _identifiablesByIdentifiers[controllerState.GetIdentifier()] = controllerState;
                }


            foreach (var identifier in _identifiablesByIdentifiers.Keys)
                _existingIdentifiers.Add(identifier);
        }

        private bool DoContainsState(string identifier) => _identifiablesByIdentifiers.TryGetValue(identifier, out var identifiable) && identifiable is ControllerState;
    }
}