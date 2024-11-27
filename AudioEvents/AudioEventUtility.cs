using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Audio
{
    public static class AudioEventUtility
    {
        private static readonly Dictionary<string, AudioReverbValueSet> PresetsByNames = new()
        {
            [nameof(AudioReverbPreset.Generic)]             = new AudioReverbValueSet(AudioReverbPreset.Generic, 0, -1000, -100, 0, 1.49f, 0.83f, -2602, 0, 200, 0.011f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.PaddedCell)]          = new AudioReverbValueSet(AudioReverbPreset.PaddedCell, 0, -1000, -6000, 0, 0.17f, 0.1f, -1204, 0, 207, 0.002f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Room)]                = new AudioReverbValueSet(AudioReverbPreset.Room, 0, -1000, -454, 0, 0.4f, 0.83f, -1646, 0, 53, 0.003f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Bathroom)]            = new AudioReverbValueSet(AudioReverbPreset.Bathroom, 0, -1000, -1200, 0, 1.49f, 0.54f, -370, 0, 1030, 0.011f, 5000, 250, 100, 60),
            [nameof(AudioReverbPreset.Livingroom)]          = new AudioReverbValueSet(AudioReverbPreset.Livingroom, 0, -1000, -6000, 0, 0.5f, 0.1f, -1376, 0, -1104, 0.004f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Stoneroom)]           = new AudioReverbValueSet(AudioReverbPreset.Stoneroom, 0, -1000, -300, 0, 2.31f, 0.64f, -711, 0, 83, 0.017f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Auditorium)]          = new AudioReverbValueSet(AudioReverbPreset.Auditorium, 0, -1000, -476, 0, 4.32f, 0.59f, -789, 0, 289, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Concerthall)]         = new AudioReverbValueSet(AudioReverbPreset.Concerthall, 0, -1000, -500, 0, 3.92f, 0.7f, -1230, 0, -2, 0.029f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Cave)]                = new AudioReverbValueSet(AudioReverbPreset.Cave, 0, -1000, 0, 0, 2.91f, 1.3f, -602, 0, 302, 0.022f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Arena)]               = new AudioReverbValueSet(AudioReverbPreset.Arena, 0, -1000, -698, 0, 7.24f, 0.33f, -1166, 0, 16, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Hangar)]              = new AudioReverbValueSet(AudioReverbPreset.Hangar, 0, -1000, -1000, 0, 10.05f, 0.23f, -602, 0, 198, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.CarpetedHallway)]     = new AudioReverbValueSet(AudioReverbPreset.CarpetedHallway, 0, -1000, -4000, 0, 0.3f, 0.1f, -1831, 0, -1630, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Hallway)]             = new AudioReverbValueSet(AudioReverbPreset.Hallway, 0, -1000, -300, 0, 1.49f, 0.59f, -1219, 0, 441, 0.011f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.StoneCorridor)]       = new AudioReverbValueSet(AudioReverbPreset.StoneCorridor, 0, -1000, -237, 0, 2.7f, 0.79f, -1214, 0, 395, 0.02f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Alley)]               = new AudioReverbValueSet(AudioReverbPreset.Alley, 0, -1000, -270, 0, 1.49f, 0.86f, -1204, 0, 4, 0.011f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Forest)]              = new AudioReverbValueSet(AudioReverbPreset.Forest, 0, -1000, -3300, 0, 1.49f, 0.54f, -2560, 0, -229, 0.088f, 5000, 250, 79, 100),
            [nameof(AudioReverbPreset.City)]                = new AudioReverbValueSet(AudioReverbPreset.City, 0, -1000, -800, 0, 1.49f, 0.67f, -2273, 0, -1691, 0.011f, 5000, 250, 50, 100),
            [nameof(AudioReverbPreset.Mountains)]           = new AudioReverbValueSet(AudioReverbPreset.Mountains, 0, -1000, -2500, 0, 1.49f, 0.21f, -2780, 0, -1434, 0.1f, 5000, 250, 27, 100),
            [nameof(AudioReverbPreset.Quarry)]              = new AudioReverbValueSet(AudioReverbPreset.Quarry, 0, -1000, -1000, 0, 1.49f, 0.83f, -10000, 0, 500, 0.025f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Plain)]               = new AudioReverbValueSet(AudioReverbPreset.Plain, 0, -1000, -2000, 0, 1.49f, 0.5f, -2466, 0, -1926, 0.1f, 5000, 250, 21, 100),
            [nameof(AudioReverbPreset.ParkingLot)]          = new AudioReverbValueSet(AudioReverbPreset.ParkingLot, 0, -1000, 0, 0, 1.65f, 1.5f, -1363, 0, 1153, 0.012f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.SewerPipe)]           = new AudioReverbValueSet(AudioReverbPreset.SewerPipe, 0, -1000, -1000, 0, 2.81f, 0.14f, -429, 0, 1023, 0.021f, 5000, 250, 80, 60),
            [nameof(AudioReverbPreset.Underwater)]          = new AudioReverbValueSet(AudioReverbPreset.Underwater, 0, -1000, -4000, 0, 1.49f, 0.1f, -449, 0, 1700, 0.011f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Drugged)]             = new AudioReverbValueSet(AudioReverbPreset.Drugged, 0, -1000, 0, 0, 8.39f, 1.39f, -115, 0, 985, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Dizzy)]               = new AudioReverbValueSet(AudioReverbPreset.Dizzy, 0, -1000, -400, 0, 17.23f, 0.56f, -1713, 0, -613, 0.03f, 5000, 250, 100, 100),
            [nameof(AudioReverbPreset.Psychotic)]           = new AudioReverbValueSet(AudioReverbPreset.Psychotic, 0, -1000, -161, 0, 7.56f, 0.91f, -626, 0, 774, 0.03f, 5000, 250, 100, 100),

        };

        public static AudioReverbValueSet GetReverbPresetValues(AudioReverbPreset preset)
        {
            var name = Enum.GetName(preset.GetType(), preset);
            return name != null ? PresetsByNames[name] : default;
        }
        public static void ApplyAudioReverbValues(AudioReverbFilter audioReverb, IAudioReverbTemplate template)
        {
            audioReverb.reverbPreset = template.Preset;
            audioReverb.dryLevel = template.DryLevel;
            audioReverb.room = template.Room;
            audioReverb.roomHF = template.RoomHF;
            audioReverb.roomLF = template.RoomLF;
            audioReverb.decayTime = template.DecayTime;
            audioReverb.decayHFRatio = template.DecayHFRatio;
            audioReverb.reflectionsLevel = template.ReflectionsLevel;
            audioReverb.reflectionsDelay = template.ReflectionsDelay;
            audioReverb.reverbLevel = template.ReverbLevel;
            audioReverb.reverbDelay = template.ReverbDelay;
            audioReverb.hfReference = template.HFReference;
            audioReverb.lfReference = template.LFReference;
            audioReverb.diffusion = template.Diffusion;
            audioReverb.density = template.Density;
        }
    }
}