using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace UnitySimplified.Serialization
{
    public sealed class VideoPlayerDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => typeof(MonoBehaviour).IsAssignableFrom(objectType);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as VideoPlayer;
            if (obj == null)
                return;

            output["isPlaying"] = obj.isPlaying;
            if (obj.isPlaying)
                output["time"] = obj.time;

            DataConvertUtility.TrySerializeFieldAsset("clip", obj.clip, output);
            DataConvertUtility.SerializeValueAsReference("camera", obj.targetCamera, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as VideoPlayer;
            if (obj == null)
                return null;

            if (input.TryGetValue("isPlaying", out object isPlaying) && (bool)isPlaying)
            {
                obj.Play();
                if (input.TryGetValue("time", out object time))
                    obj.time = (double)time;
            }

            if (DataConvertUtility.TryDeserializeFieldAsset("clip", out object asset, input))
                obj.clip = asset as VideoClip;
            DataConvertUtility.DeserializeValueFromReference("camera", x => obj.targetCamera = (Camera)x, input);

            return obj;
        }
    }
}