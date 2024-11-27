using UnityEngine;
using UnityEngine.Audio;

namespace UnitySimplified.Audio
{
    public interface IAudioSourceTemplate
    {
        AudioClip Clip { get; }
        AudioMixerGroup MixerGroup { get; }
        float Volume { get; }
        float Pitch { get; }
        bool Loop { get; }
    }
}