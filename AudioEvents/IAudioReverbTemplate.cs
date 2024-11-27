using UnityEngine;

namespace UnitySimplified.Audio
{
    public interface IAudioReverbTemplate
    {
        AudioReverbPreset Preset { get; }
        float DryLevel { get; }
        float Room { get; }
        float RoomHF { get; }
        float RoomLF { get; }
        float DecayTime { get; }
        float DecayHFRatio { get; }
        float ReflectionsLevel { get; }
        float ReflectionsDelay { get; }
        float ReverbLevel { get; }
        float ReverbDelay { get; }
        float HFReference { get; }
        float LFReference { get; }
        float Diffusion { get; }
        float Density { get; }
    }
}