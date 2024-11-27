using UnityEngine;
using UnityEngine.Audio;

namespace UnitySimplified.Audio
{
    [CreateAssetMenu(fileName = "New Simple Audio Event", menuName = "Unity Simplified/Audio/Events/Simple")]
    public class SimpleAudioEvent : AudioEvent, IAudioSourceTemplate
    {
        [SerializeField]
        private AudioClip clip;
        [SerializeField]
        private AudioMixerGroup mixerGroup;

        [SerializeField] [Range(0, 1)]
        private float volume = 1;
        [SerializeField] [Range(0, 2)]
        private float pitch = 1;
        [SerializeField]
        private bool loop;

        AudioClip IAudioSourceTemplate.Clip => clip;
        AudioMixerGroup IAudioSourceTemplate.MixerGroup => mixerGroup;
        float IAudioSourceTemplate.Volume => volume;
        float IAudioSourceTemplate.Pitch => pitch;
        bool IAudioSourceTemplate.Loop => loop;

        public void Play() => Play(default);
        public void Play(Vector3 position) => Play(new AudioEventParams { Position = position });
        protected override void OnPlay(AudioEventHelper audioEventHelper, AudioEventParams audioEventParams)
        {
            var audioTemplate = this as IAudioSourceTemplate;
            if (!audioTemplate.Clip)
                return;

            audioEventHelper.AddAudioSampler(audioTemplate);
            audioEventHelper.Play();
        }
    }
}