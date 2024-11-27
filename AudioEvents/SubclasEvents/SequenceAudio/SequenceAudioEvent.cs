using System;
using UnityEngine;

namespace UnitySimplified.Audio
{
    [CreateAssetMenu(fileName = "New SequenceAudioEvent", menuName = "Unity Simplified/Audio/Sequence Event")]
    public class SequenceAudioEvent : AudioEvent
    {
        [SerializeField]
        private AnimationCurve fadeInCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
        [SerializeField]
        private AnimationCurve fadeOutCurve = new(new Keyframe(0, 1), new Keyframe(1, 0));
        [SerializeField]
        private SequenceAudioEntry[] entries = Array.Empty<SequenceAudioEntry>();

        public AnimationCurve FadeInCurve => fadeInCurve;
        public AnimationCurve FadeOutCurve => fadeOutCurve;
        public SequenceAudioEntry[] Entries => entries;

        public void Play() => Play(default);
        public void Play(Vector3 position) => Play(new AudioEventParams { Position = position });
        public void Play(Vector3 position, Transform parent) => Play(new AudioEventParams { Position = position, Parent = parent} );
        protected override void OnPlay(AudioEventHelper audioEventHelper, AudioEventParams audioEventParams)
        {
            foreach (var entry in entries)
            {
                var audioSampler = audioEventHelper.AddAudioSampler(entry.Clip);
                if (entry is IAudioSourceTemplate sourceTemplate)
                {
                    audioSampler.Loop = sourceTemplate.Loop;
                    audioSampler.Volume = sourceTemplate.Volume;
                    audioSampler.Pitch = sourceTemplate.Pitch;
                    audioSampler.MixerGroup = sourceTemplate.MixerGroup;
                }

                if (entry is IAudioReverbTemplate reverbTemplate)
                {
                    if (audioSampler.Sampler.TryGetComponent(out AudioReverbFilter audioReverb))
                    {
                        AudioEventUtility.ApplyAudioReverbValues(audioReverb, reverbTemplate);
                    }
                }
            }

            audioEventHelper.AddAudioTrackFade(fadeInCurve, fadeOutCurve);
            audioEventHelper.Play();
        }
    }
}