using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace UnitySimplified.Serialization
{
    public sealed class VideoPlayerDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => typeof(MonoBehaviour).IsAssignableFrom(objectType);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output["isPlaying"] = obj.isPlaying;
                if (obj.isPlaying)
                    output["time"] = obj.time;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset("clip", obj.clip, output);
                if (flags.HasFlag(SerializerFlags.RuntimeReference))
                    DataConvertUtility.SerializeValueAsReference("camera", obj.targetCamera, output);
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue("isPlaying", out object isPlaying) && (bool)isPlaying)
                {
                    obj.Play();
                    if (input.TryGetValue("time", out object time))
                        obj.time = (double)time;
                }

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset("clip", out object asset, typeof(VideoClip), input))
                        obj.clip = asset as VideoClip;
                if (flags.HasFlag(SerializerFlags.RuntimeReference))
                    DataConvertUtility.DeserializeValueFromReference("camera", x => obj.targetCamera = (Camera)x, input);
            }
        }
    }
}