using System;

namespace UnitySimplified.Audio
{
    public sealed partial class AudioEventHelper
    {
        public class Operation
        {
            private readonly Action _onPlay;
            private readonly Action _onStop;
            private readonly Action _onStopImmediately;
            private readonly Action _onUpdate;

            public Operation(AudioEventHelper helper, Action onPlay = default, Action onStop = default, Action onStopImmediately = default, Action onUpdate = default)
            {
                Helper = helper;
                _onPlay = onPlay;
                _onStop = onStop;
                _onStopImmediately = onStopImmediately;
                _onUpdate = onUpdate;
            }

            public AudioEventHelper Helper { get; }
            public bool IsPlaying { get; private set; }
            public bool IsStopping { get; private set; }

            internal void Play()
            {
                if (IsPlaying)
                    return;
                IsPlaying = true;
                OnPlay();
                _onPlay?.Invoke();
            }
            internal void Stop()
            {
                if (!IsPlaying)
                    return;
                if (IsStopping)
                    return;
                IsStopping = true;
                OnStop();
                _onStop?.Invoke();
            }
            internal void StopImmediately()
            {
                if (!IsPlaying)
                    return;
                IsPlaying = false;
                IsStopping = false;
                OnStopImmediately();
                _onStopImmediately?.Invoke();
            }
            internal void Update()
            {
                if (!IsPlaying)
                    return;
                OnUpdate();
                _onUpdate?.Invoke();
            }

            protected void Kill() => StopImmediately();

            protected virtual void OnPlay() { }
            protected virtual void OnStop() { }
            protected virtual void OnStopImmediately() { }
            protected virtual void OnUpdate() { }
        }
    }
}