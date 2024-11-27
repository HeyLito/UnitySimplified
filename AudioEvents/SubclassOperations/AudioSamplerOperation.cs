using UnityEngine;
using UnityEngine.Audio;

namespace UnitySimplified.Audio
{
    public class AudioSamplerOperation : AudioEventHelper.Operation
    {
        private AudioMixerGroup _mixerGroup;
        internal AudioSamplerOperation(AudioEventHelper helper, AudioClip audioClip) : base(helper)
        {
            var samplerObject = new GameObject($"{audioClip.name}.AudioSampler", typeof(AudioSource));
            Sampler = samplerObject.GetComponent<AudioSource>();
            Sampler.transform.SetParent(helper.transform);
            Sampler.Stop();
            Sampler.clip = audioClip;
            Sampler.loop = false;
            Volume = Sampler.volume;
        }

        public bool Loop { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public AudioSource Sampler { get; }
        public AudioMixerGroup MixerGroup
        {
            get => _mixerGroup;
            set => _mixerGroup = Sampler.outputAudioMixerGroup = value;
        }

        protected override void OnPlay() => Sampler.Play();
        protected override void OnStop() { }
        protected override void OnStopImmediately() => Sampler.Stop();
        protected override void OnUpdate()
        {
            Sampler.volume = Volume * Helper.Volume;
            Sampler.pitch = Pitch;
            if (Sampler.isPlaying)
                return;

            if (Loop && !IsStopping)
                Sampler.Play();
            else Kill();
        }
    }
    public static class AudioSamplerOperationExtensions
    {
        public static AudioSamplerOperation AddAudioSampler(this AudioEventHelper helper, AudioClip audioClip)
        {
            AudioSamplerOperation operation = new(helper, audioClip);
            helper.AppendOperation(operation);
            return operation;
        }
        public static AudioSamplerOperation AddAudioSampler(this AudioEventHelper helper, IAudioSourceTemplate template)
        {
            AudioSamplerOperation operation = new(helper, template.Clip)
            {
                Loop = template.Loop,
                Volume = template.Volume,
                Pitch = template.Pitch,
                MixerGroup = template.MixerGroup
            };
            helper.AppendOperation(operation);
            return operation;
        }
        public static AudioSamplerOperation SetMixerGroup(this AudioSamplerOperation operation, AudioMixerGroup value)
        {
            operation.MixerGroup = value;
            return operation;
        }
        public static AudioSamplerOperation SetVolume(this AudioSamplerOperation operation, float value)
        {
            operation.Volume = value;
            return operation;
        }
        public static AudioSamplerOperation SetPitch(this AudioSamplerOperation operation, float value)
        {
            operation.Pitch = value;
            return operation;
        }
        public static AudioSamplerOperation SetLoop(this AudioSamplerOperation operation, bool value)
        {
            operation.Loop = value;
            return operation;
        }
    }
}