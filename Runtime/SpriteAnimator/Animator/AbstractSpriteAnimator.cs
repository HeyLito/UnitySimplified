using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// ReSharper disable ParameterHidesMember

namespace UnitySimplified.SpriteAnimator
{
    [DisallowMultipleComponent]
    public abstract class AbstractSpriteAnimator : MonoBehaviour
    {
        public delegate void PlayCallbackDelegate();
        public delegate void StopCallbackDelegate();
        public delegate void SpriteUpdatedCallbackDelegate(Sprite sprite);
        internal delegate void AnimationConditionCallbackDelegate(AnimationCondition condition);

        #region FIELDS
        [NonSerialized]
        internal AnimationConditionCallbackDelegate onAnyAnimationConditionAddedCallback;
        [NonSerialized]
        internal AnimationConditionCallbackDelegate onAnyAnimationConditionRemovedCallback;
        [NonSerialized]
        private bool _initialized;
        [NonSerialized]
        private readonly HashSet<AnimationState> _animationStates = new();
        [NonSerialized]
        private readonly List<AnimationState> _orderedAnimationStates = new();
        [NonSerialized]
        private readonly Dictionary<string, AnimationState> _animationStatesByIdentifiers = new();
        [NonSerialized]
        private readonly Dictionary<string, HashSet<SpriteAnimation.EventTriggerReceiver>> _triggerReceiversByIdentifiers = new();
        [NonSerialized]
        private (AnimationState state, int? frame, Coroutine sequence) _current = (null, null, null);
        [NonSerialized]
        private (AnimationState state, int? startFrame) _next = (null, null);
        [NonSerialized]
        private readonly Queue<SpriteAnimation.EventTriggerReceiver> _tempEventTriggerReceivers = new();
        [NonSerialized]
        private PlayCallbackDelegate _onPlayCallback;
        [NonSerialized]
        private StopCallbackDelegate _onStopCallback;
        [NonSerialized]
        private SpriteUpdatedCallbackDelegate _onSpriteUpdatedCallback;
        #endregion


        #region EVENTS
        public event PlayCallbackDelegate OnPlayCallback
        {
            add { lock (this) _onPlayCallback += value; }
            remove { lock (this) _onPlayCallback -= value; }
        }
        public event StopCallbackDelegate OnStopCallback
        {
            add { lock (this) _onStopCallback += value; }
            remove { lock (this) _onStopCallback -= value; }
        }
        public event SpriteUpdatedCallbackDelegate OnSpriteUpdatedCallback
        {
            add { lock (this) _onSpriteUpdatedCallback += value; }
            remove { lock (this) _onSpriteUpdatedCallback -= value; }
        }
        #endregion


        #region PROPERTIES
        public abstract Sprite Sprite { get; protected set; }
        public (AnimationState state, int? frame) Current => (_current.state, _current.frame);
        public (AnimationState state, int? startFrame) Next => _next;
        public IReadOnlyCollection<AnimationState> AnimationStates => _orderedAnimationStates;
        #endregion


        #region METHODS_PROTECTED_MESSAGES
        protected virtual void OnDisable() => Stop();
        protected virtual void OnInitialize() { }
        #endregion

        #region METHODS_PUBLIC
        public bool IsPlaying() => _current != (null, null, null);
        public bool IsPlaying(string stateName) => _current != (null, null, null) && _current.state.Identifier == stateName;

        public bool AddAnimationState(AnimationState animationState)
        {
            Initialize();
            if (!DoVerifyAnimationState(animationState, nameof(animationState), out var exception))
                throw exception;

            DoAddAnimationState(animationState);
            return true;
        }
        public bool RemoveAnimationState(AnimationState animationState)
        {
            Initialize();
            if (!DoVerifyAnimationState(animationState, nameof(animationState), out var exception))
                throw exception;

            return DoRemoveAnimationState(animationState);
        }

        public AnimationState CreateAnimationState(string identifier, SpriteAnimation animation, InterruptionSource interruptionSource = InterruptionSource.AnyState) => DoCreateAnimationState(typeof(AnimationState), identifier, animation, interruptionSource);
        public AnimationState CreateAnimationState<T>(string identifier, SpriteAnimation animation, InterruptionSource interruptionSource = InterruptionSource.AnyState) where T : AnimationState => DoCreateAnimationState(typeof(T), identifier, animation, interruptionSource);
        public AnimationState CreateAnimationState(Type animationStateType, string identifier, SpriteAnimation animation, InterruptionSource interruptionSource = InterruptionSource.AnyState) => DoCreateAnimationState(animationStateType, identifier, animation, interruptionSource);
        public bool Contains(AnimationState state) => _animationStates.Contains(state);
        public void Stop()
        {
            if (_current == (null, null, null))
                return;

            if (_current.sequence != null)
            {
                _onStopCallback?.Invoke();
                StopCoroutine(_current.sequence);
            }
            _current = (null, null, null);
        }
        public void Restart()
        {
            Stop();
            Play();
        }
        public void SlipPlay(SpriteAnimation animation)
        {
            if (animation == null)
                throw new ArgumentNullException(nameof(animation));

            if (TryGetAnimationState("_SlipPlay", out AnimationState animationState))
                RemoveAnimationState(animationState);
            var state = CreateAnimationState("_SlipPlay", animation);
            ForcePlay(state);
        }
        public void Play()
        {
            Initialize();
            foreach (var state in AnimationStates)
                if (state is GlobalAnimationState globalState)
                    if (globalState.OnTryPlay(this))
                        return;
        }
        public bool Play(string stateName)
        {
            if (TryGetAnimationState(stateName, out var animationState))
                return DoPlay(animationState, 0, false);

            Debug.LogWarning($"Could not play AnimationState \"{name}\" because it does not exist!");
            return false;
        }
        public bool Play(string stateName, int startFrame)
        {
            if (TryGetAnimationState(stateName, out AnimationState animationState))
                return DoPlay(animationState, startFrame, false);

            Debug.LogWarning($"Could not play AnimationState \"{name}\" because it does not exist!");
            return false;
        }
        public bool Play(AnimationState animationState)
        {
            if (animationState != null)
                return DoPlay(animationState, 0, false);

            throw new ArgumentNullException($"Could not play {nameof(animationState)} because it is null!");
        }
        public bool Play(AnimationState animationState, int startFrame)
        {
            if (animationState != null)
                return DoPlay(animationState, startFrame, false);

            throw new ArgumentNullException($"Could not play {nameof(animationState)} because it is null!");
        }
        public void PlayNext(AnimationState animationState, int startFrame)
        {
            if (animationState != null)
            {
                if (_next == (null, null))
                {
                    _next.state = animationState;
                    _next.startFrame = startFrame;
                }
                else Debug.LogWarning($"Could not set AnimationState \"{animationState.Identifier}\" as next another state has already been set!");
            }
            else throw new ArgumentNullException($"Could not set {nameof(animationState)} as next because it is null!");
        }
        public void ForcePlay(string stateName)
        {
            if (TryGetAnimationState(stateName, out var animationState))
                DoPlay(animationState, 0, true);
            else Debug.LogWarning($"Could not play AnimationState \"{name}\" because it does not exist!");
        }
        public void ForcePlay(string stateName, int startFrame)
        {
            if (TryGetAnimationState(stateName, out AnimationState animationState))
                DoPlay(animationState, startFrame, true);
            else Debug.LogWarning($"Could not play AnimationState \"{name}\" because it does not exist!");
        }
        public void ForcePlay(AnimationState animationState)
        {
            if (animationState != null)
                DoPlay(animationState, 0, true);
            else throw new ArgumentNullException($"Could not play {nameof(animationState)} because it is null!");
        }
        public void ForcePlay(AnimationState animationState, int startFrame)
        {
            if (animationState != null)
                DoPlay(animationState, startFrame, true);
            else throw new ArgumentNullException($"Could not play {nameof(animationState)} because it is null!");
        }
        public bool TryGetAnimationState(string stateName, out AnimationState animationState)
        {
            Initialize();
            return _animationStatesByIdentifiers.TryGetValue(stateName, out animationState);
        }
        public void AddTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver) => DoAddTriggerReceiver(triggerReceiver);
        public void RemoveTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver) => DoRemoveTriggerReceiver(triggerReceiver);
        #endregion

        #region METHODS_PRIVATE
        private void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            foreach (var state in GlobalAnimationState.States)
                DoCreateAnimationState(state.GetType(), state.Identifier, null, InterruptionSource.None);
            OnInitialize();
        }
        private AnimationState DoCreateAnimationState(Type stateType, string identifier, SpriteAnimation animation, InterruptionSource interruptionSource)
        {
            Initialize();

            if (stateType == null)
                throw new ArgumentNullException($"Could not create animation state; Method parameter {nameof(stateType)} is null.");
            var animationState = AnimationState.CreateAnimationState(stateType, identifier, this, animation, interruptionSource);

            if (!DoVerifyAnimationState(animationState, nameof(animationState), out var exception))
                throw exception;
            DoAddAnimationState(animationState);
            return animationState;
        }
        private static bool DoVerifyAnimationState(AnimationState state, string argumentName, out Exception exception)
        {
            exception = null;
            if (state == null)
                exception = new ArgumentNullException(nameof(state));
            else if (string.IsNullOrEmpty(state.Identifier))
                exception = new ArgumentException($"The identifier in \"{argumentName}\" is empty or null.");
            return exception == null;
        }
        private void DoAddAnimationState(AnimationState state)
        {
            if (_animationStates.Contains(state))
                throw new System.Data.DuplicateNameException(nameof(state));
            if (_animationStatesByIdentifiers.ContainsKey(state.Identifier))
                throw new System.Data.DuplicateNameException(state.Identifier);

            _animationStates.Add(state);
            _animationStatesByIdentifiers.Add(state.Identifier, state);
            _orderedAnimationStates.Add(state);
            _orderedAnimationStates.Sort((x, y) => y.Priority.CompareTo(x.Priority));

            foreach (var transition in state.Transitions)
                foreach (var condition in transition.Conditions)
                    onAnyAnimationConditionAddedCallback?.Invoke(condition);
        }
        private bool DoRemoveAnimationState(AnimationState state)
        {
            if (!_animationStates.Contains(state))
                return false;
            if (!_animationStatesByIdentifiers.ContainsKey(state.Identifier))
                return false;

            _animationStates.Remove(state);
            _animationStatesByIdentifiers.Remove(state.Identifier);
            _orderedAnimationStates.Remove(state);

            Stop();

            foreach (var transition in state.Transitions)
                foreach (var condition in transition.Conditions)
                    onAnyAnimationConditionRemovedCallback?.Invoke(condition);

            return true;
        }
        private void DoAddTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver)
        {
            if (triggerReceiver != null)
            {
                if (!string.IsNullOrEmpty(triggerReceiver.Key))
                {
                    if (!_triggerReceiversByIdentifiers.TryGetValue(triggerReceiver.Key, out var triggerReceivers))
                        _triggerReceiversByIdentifiers[triggerReceiver.Key] = triggerReceivers = new HashSet<SpriteAnimation.EventTriggerReceiver>();
                    triggerReceivers.Add(triggerReceiver);
                }
                else throw new ArgumentException($"{nameof(triggerReceiver)}.{nameof(SpriteAnimation.EventTriggerReceiver.Key)} is null or empty.");
            }
            else throw new ArgumentNullException(nameof(triggerReceiver));
        }
        private void DoRemoveTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver)
        {
            if (triggerReceiver != null)
            {
                if (_triggerReceiversByIdentifiers.TryGetValue(triggerReceiver.Key, out var triggerReceivers))
                {
                    triggerReceivers.Remove(triggerReceiver);
                    if (triggerReceivers.Count == 0)
                        _triggerReceiversByIdentifiers.Remove(triggerReceiver.Key);
                }
                else throw new ArgumentNullException($"Does not contain {nameof(SpriteAnimation.EventTriggerReceiver)} with an identifier of {triggerReceiver.Key}.");
            }
            else throw new ArgumentNullException(nameof(triggerReceiver));
        }
        private bool DoPlay(AnimationState state, int startFrame, bool forcePlay)
        {
            if (!gameObject.activeInHierarchy)
                return false;

            var isPlaying = IsPlaying();
            if (isPlaying)
                if (!forcePlay)
                    return false;

            _onPlayCallback?.Invoke();
            DoPlayAnimationState(state, startFrame);
            return true;
        }
        private void DoPlayAnimationState(AnimationState state, int startFrame)
        {
            if (state == null)
                throw new NullReferenceException();
            if (IsPlaying())
                throw new InvalidOperationException($"Already playing animation! Can not play requested animation: {state.Identifier}.");

            _current.state = state;
            _current.frame = startFrame;
            _current.sequence = StartCoroutine(AnimationStateSequence(state, startFrame));

            if (state.Animation == null || state.Animation.Frames.Length <= startFrame)
                return;
            Sprite = state.Animation.Frames[startFrame];
            _onSpriteUpdatedCallback?.Invoke(Sprite);
            HandleAnimationFrameEvents(startFrame, state.Animation);
        }
        private void HandleAnimationFrameEvents(int frame, SpriteAnimation animation)
        {
            if (animation != null)
            {
                foreach (var animationTrigger in animation.Triggers)
                    if (animationTrigger.Frame == frame)
                        if (_triggerReceiversByIdentifiers.TryGetValue(animationTrigger.Key, out var eventTriggerReceivers))
                        {
                            foreach (var eventTriggerReceiver in eventTriggerReceivers)
                                _tempEventTriggerReceivers.Enqueue(eventTriggerReceiver);
                            while (_tempEventTriggerReceivers.TryDequeue(out var eventTriggerReceiver))
                                eventTriggerReceiver.Callback();
                        }
            }
            else throw new ArgumentNullException(nameof(animation));
        }
        private void NextAnimationFrame(ref int frame, SpriteAnimation animation)
        {
            frame++;
            if (frame > animation.Frames.Length - 1)
            {
                if (animation.Loop)
                {
                    frame = 0;
                    HandleAnimationFrameEvents(frame, animation);
                }
                else frame = animation.Frames.Length - 1;
            }
            else HandleAnimationFrameEvents(frame, animation);
        }
        private IEnumerator AnimationStateSequence(AnimationState state, int startFrame)
        {
            var frame = startFrame;
            float elapsedTime = 0, frameTime = 0, targetFrameTime = 0;
            if (state.Animation != null)
                targetFrameTime = 1 / (float)state.Animation.Fps;

            foreach (var otherState in _animationStates)
                otherState.OnSequenceEntered(state);

            var loop = true;
            while (loop)
            {
                if (_next != (null, null))
                    loop = false;
                if (targetFrameTime == 0)
                {
                    if (_next == (null, null) && state.TryGetNext(elapsedTime, out AnimationTransition next))
                        PlayNext(next.OutState, next.FrameOffset);
                    loop = false;
                }

                while (frameTime < targetFrameTime && loop)
                {
                    frameTime += Time.deltaTime;
                    yield return null;
                }
                while (frameTime >= targetFrameTime && loop)
                {
                    elapsedTime += targetFrameTime;
                    frameTime -= targetFrameTime;

                    //foreach (var otherState in _animationStates)
                    //    otherState.OnSequenceUpdate(state, elapsedTime);

                    if (_next != (null, null))
                        loop = false;
                    else if (state.TryGetNext(elapsedTime, out AnimationTransition next))
                    {
                        PlayNext(next.OutState, next.FrameOffset);
                        loop = false;
                    }

                    foreach (var otherState in _animationStates)
                        otherState.OnSequenceUpdate(state, elapsedTime);

                    if (state.Animation != null)
                    {
                        NextAnimationFrame(ref frame, state.Animation);
                        _current.frame = frame;
                    }
                }

                if (state.Animation != null && state.Animation.Frames.Length > frame)
                {
                    Sprite = state.Animation?.Frames[frame];
                    _onSpriteUpdatedCallback?.Invoke(Sprite);
                }

                if (!loop)
                    yield return null;
            }

            foreach (var otherState in _animationStates)
                otherState.OnSequenceExited(state);

            var cachedNext = _next;
            _next = (null, null);
            _current = (null, null, null);
            if (cachedNext != (null, null))
                DoPlayAnimationState(cachedNext.state, cachedNext.startFrame ?? cachedNext.startFrame.Value);
        }
        #endregion
    }
}
