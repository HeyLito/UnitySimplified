using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityObject = UnityEngine.Object;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(VideoPlayer), -1)]
    public sealed class VideoPlayerSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData["isPlaying"] = obj.isPlaying;
                if (obj.isPlaying)
                    fieldData["time"] = obj.time;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset("clip", obj.clip, fieldData);
                
                if (flags.HasFlag(SerializerFlags.RuntimeReference))
                    DataSerializer.AddPostSerializerAction(delegate { DataSerializerUtility.SerializeFieldReference("camera", obj.targetCamera, fieldData, typeof(UnityObject)); });
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as VideoPlayer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue("isPlaying", out object isPlaying) && (bool)isPlaying)
                {
                    obj.Play();
                    if (fieldData.TryGetValue("time", out object time))
                        obj.time = (double)time;
                }
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset("clip", out object asset, typeof(VideoClip), fieldData))
                        obj.clip = asset as VideoClip;

                if (flags.HasFlag(SerializerFlags.RuntimeReference))
                {
                    object reference = null;
                    DataSerializer.AddPostSerializerAction(delegate { DataSerializerUtility.DeserializeFieldReference("camera", out reference, fieldData); });
                    DataSerializer.AddPostSerializerAction(delegate { obj.targetCamera = (Camera)reference; });
                }
            }
        }
    }
}