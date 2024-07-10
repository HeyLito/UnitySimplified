using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.SpriteAnimator
{
    [DisallowMultipleComponent]
    public abstract class BaseSpriteAnimator : MonoBehaviour
    {
        public delegate void SpriteUpdateCallbackDelegate(Sprite sprite);

        #region FIELDS
        [NonSerialized]
        internal Action<AnimationCondition> onAnyConditionAdded;
        [NonSerialized]
        internal Action<AnimationCondition> onAnyConditionRemoved;

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
        private event SpriteUpdateCallbackDelegate _onSpriteUpdateCallback = null;
        #endregion


        #region PROPERTIES
        public abstract Sprite Sprite { get; protected set; }
        public (AnimationState state, int? frame) Current => (_current.state, _current.frame);
        public (AnimationState state, int? startFrame) Next => _next;
        public IReadOnlyCollection<AnimationState> AnimationStates => _orderedAnimationStates;
        public event SpriteUpdateCallbackDelegate OnSpriteUpdateCallback
        {
            add { lock (this) _onSpriteUpdateCallback += value; }
            remove { lock (this) _onSpriteUpdateCallback -= value; }
        }
        #endregion


        #region METHODS_PROTECTED_EVENTS
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
                StopCoroutine(_current.sequence);
            _current = (null, null, null);
        }
        //public void Restart()
        //{
        //    //if (DoTransitionToNextAnimationState(GetAnimationStateEntry(), 0, 0, out AnimationState animationState))
        //    //    DoPlayAnimation(animationState, 0);
        //}
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
                    onAnyConditionAdded?.Invoke(condition);
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

            foreach (var transition in state.Transitions)
                foreach (var condition in transition.Conditions)
                    onAnyConditionRemoved?.Invoke(condition);

            return true;
        }
        private void DoAddTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver)
        {
            if (triggerReceiver != null)
            {
                if (!string.IsNullOrEmpty(triggerReceiver.Identifier))
                {
                    if (!_triggerReceiversByIdentifiers.TryGetValue(triggerReceiver.Identifier, out var triggerReceivers))
                        _triggerReceiversByIdentifiers[triggerReceiver.Identifier] = triggerReceivers = new();
                    triggerReceivers.Add(triggerReceiver);
                }
                else throw new ArgumentException($"{nameof(triggerReceiver)}.{nameof(SpriteAnimation.EventTriggerReceiver.Identifier)} is null or empty.");
            }
            else throw new ArgumentNullException(nameof(triggerReceiver));
        }
        private void DoRemoveTriggerReceiver(SpriteAnimation.EventTriggerReceiver triggerReceiver)
        {
            if (triggerReceiver != null)
            {
                if (_triggerReceiversByIdentifiers.TryGetValue(triggerReceiver.Identifier, out var triggerReceivers))
                {
                    triggerReceivers.Remove(triggerReceiver);
                    if (triggerReceivers.Count == 0)
                        _triggerReceiversByIdentifiers.Remove(triggerReceiver.Identifier);
                }
                else throw new ArgumentNullException($"Does not contain {nameof(SpriteAnimation.EventTriggerReceiver)} with an identifier of {triggerReceiver.Identifier}.");
            }
            else throw new ArgumentNullException(nameof(triggerReceiver));
        }
        private bool DoPlay(AnimationState state, int startFrame, bool forcePlay)
        {
            var isPlaying = IsPlaying();
            if (isPlaying)
                if (!forcePlay)
                    return false;
            
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
            _onSpriteUpdateCallback?.Invoke(Sprite);
            HandleAnimationFrameEvents(startFrame, state.Animation);
        }
        private void HandleAnimationFrameEvents(int frame, SpriteAnimation animation)
        {
            if (animation != null)
            {
                foreach (var animationTrigger in animation.Triggers)
                    if (animationTrigger.Frame == frame)
                        if (_triggerReceiversByIdentifiers.TryGetValue(animationTrigger.Identifier, out var eventTriggerReceivers))
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
            HandleAnimationFrameEvents(Mathf.Min(frame, Mathf.Max(0, animation.Frames.Length - 1)), animation);

            if (frame < animation.Frames.Length)
                return;
            if (animation.Loop)
                frame = 0;
            else frame = animation.Frames.Length - 1;
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
                        PlayNext(next.OutState, next.TransitionOffset);
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
                        PlayNext(next.OutState, next.TransitionOffset);
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
                    _onSpriteUpdateCallback?.Invoke(Sprite);
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
