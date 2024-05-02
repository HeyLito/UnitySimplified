using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MeshFilterDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MeshFilter);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                DataConvertUtility.SerializeFieldAsset(nameof(MeshFilter.sharedMesh), obj.sharedMesh, output);
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                if (DataConvertUtility.DeserializeFieldAsset(nameof(MeshFilter.sharedMesh), out object sharedMesh, typeof(MeshFilter), input))
                    obj.mesh = sharedMesh as Mesh;
        }
    }
}