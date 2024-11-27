using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnitySimplifiedEditor;
#endif

namespace UnitySimplified.Audio
{
    public class AudioTrackFadeOperation : AudioEventHelper.Operation
    {
        private float _elapsed;
        private AnimationCurve _activeAnimationCurve;
        public AudioTrackFadeOperation(AudioEventHelper helper, AnimationCurve fadeInCurve = default, AnimationCurve fadeOutCurve = default) : base(helper)
        {
            if (fadeInCurve != null)
                FadeInCurve = fadeInCurve;
            if (fadeOutCurve != null)
                FadeOutCurve = fadeOutCurve;
        }

        public AnimationCurve FadeInCurve { get; set; } = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve FadeOutCurve { get; set; } = AnimationCurve.Linear(0, 1, 1, 0);

        protected override void OnPlay()
        {
            _activeAnimationCurve = FadeInCurve;
            _elapsed = 0;
        }

        protected override void OnStop()
        {
            _activeAnimationCurve = FadeOutCurve;
            _elapsed = 0;
        }

        protected override void OnStopImmediately()
        {
            _activeAnimationCurve = null;
            _elapsed = 0;
        }

        protected override void OnUpdate()
        {
            if (_activeAnimationCurve == null)
                return;
            float targetTime = _activeAnimationCurve[_activeAnimationCurve.length - 1].time;
            Helper.Volume = _activeAnimationCurve.Evaluate(_elapsed);
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
                _elapsed += EditorApplicationUtility.DeltaTime;
            else _elapsed += Time.deltaTime;
#else
            elapsed += Time.deltaTime;
#endif
            if (!(targetTime < _elapsed))
                return;
            Helper.Volume = _activeAnimationCurve.Evaluate(targetTime);
            if (_activeAnimationCurve.Equals(FadeOutCurve))
                Helper.StopImmediately();
        }
    }

    public static class AudioTrackFadeOperationExtensions
    {
        public static AudioTrackFadeOperation AddAudioTrackFade(this AudioEventHelper helper, AnimationCurve fadeInCurve = default, AnimationCurve fadeOutCurve = default)
        {
            AudioTrackFadeOperation operation = new(helper, fadeInCurve, fadeOutCurve);
            helper.AppendOperation(operation);
            return operation;
        }
    }
}