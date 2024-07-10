using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MeshFilterDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MeshFilter);

        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as MeshFilter;
            if (obj == null)
                return;

            DataConvertUtility.TrySerializeFieldAsset(nameof(MeshFilter.sharedMesh), obj.sharedMesh, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as MeshFilter;
            if (obj == null)
                return null;

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(MeshFilter.sharedMesh), out object sharedMesh, input))
                obj.mesh = sharedMesh as Mesh;

            return obj;
        }
    }
}