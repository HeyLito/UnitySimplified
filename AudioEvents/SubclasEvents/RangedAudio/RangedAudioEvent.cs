using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace UnitySimplified.Audio
{
    [CreateAssetMenu(fileName = "New Ranged Audio Event", menuName = "Unity Simplified/Audio/Events/Ranged")]
    public class RangedAudioEvent : AudioEvent, IAudioSourceTemplate
    {
        [SerializeField]
        private AudioClip clip;
        [SerializeField]
        private AudioMixerGroup mixerGroup;

        [SerializeField, CustomRange(0, 1)]
        private RangedFloat volume = new(1, 1);
        [SerializeField, CustomRange(0, 2)]
        private RangedFloat pitch = new(1, 1);
        [SerializeField] private bool loop;

        AudioClip IAudioSourceTemplate.Clip => clip;
        AudioMixerGroup IAudioSourceTemplate.MixerGroup => mixerGroup;
        float IAudioSourceTemplate.Volume => Random.Range(volume.min, volume.max);
        float IAudioSourceTemplate.Pitch => Random.Range(pitch.min, pitch.max);
        bool IAudioSourceTemplate.Loop => loop;

        public void Play() => Play(default);
        public void Play(Vector3 position) => Play(new AudioEventParams { Position = position });
        public void Play(Vector3 position, Transform parent) => Play(new AudioEventParams { Position = position, Parent = parent});
        protected override void OnPlay(AudioEventHelper audioEventHelper, AudioEventParams audioParams)
        {
            var audioTemplate = this as IAudioSourceTemplate;
            if (!audioTemplate.Clip)
                return;

            audioEventHelper.AddAudioSampler(audioTemplate);
            audioEventHelper.Play();
        }
    }
}