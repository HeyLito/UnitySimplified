using System;
using UnityEngine;
using UnityEngine.Audio;

namespace UnitySimplified.Audio
{
    [Serializable]
    public class SequenceAudioEntry : IAudioSourceTemplate, IAudioReverbTemplate
    {
        [SerializeField]
        private bool enabled = true;
        [SerializeField]
        private AudioSourceValueSet sourceValues = new(false, 0.5f, 1);
        [SerializeField]
        private AudioReverbValueSet reverbValues = new(AudioReverbPreset.Generic, true);
        [SerializeField]
        private bool randomizedAudio;
        [SerializeField]
        private float randomMinStart;
        [SerializeField]
        private float randomMaxStart;

        public bool Enabled => enabled;
        public bool RandomizedAudio => randomizedAudio;
        public float RandomMinStart => randomMinStart;
        public float RandomMaxStart => randomMaxStart;

        public AudioClip Clip => sourceValues.Clip;
        public AudioMixerGroup MixerGroup => sourceValues.MixerGroup;
        public float Volume => sourceValues.Volume;
        public float Pitch => sourceValues.Pitch;
        public bool Loop => sourceValues.Loop && !randomizedAudio;

        public AudioReverbPreset Preset => reverbValues.Preset;
        public float DryLevel => reverbValues.DryLevel;
        public float Room => reverbValues.Room;
        public float RoomHF => reverbValues.RoomHF;
        public float RoomLF => reverbValues.RoomLF;
        public float DecayTime => reverbValues.DecayTime;
        public float DecayHFRatio => reverbValues.DecayHFRatio;
        public float ReflectionsLevel => reverbValues.ReflectionsLevel;
        public float ReflectionsDelay => reverbValues.ReflectionsDelay;
        public float ReverbLevel => reverbValues.ReverbLevel;
        public float ReverbDelay => reverbValues.ReverbDelay;
        public float HFReference => reverbValues.HFReference;
        public float LFReference => reverbValues.LFReference;
        public float Diffusion => reverbValues.Diffusion;
        public float Density => reverbValues.Density;
    }
}