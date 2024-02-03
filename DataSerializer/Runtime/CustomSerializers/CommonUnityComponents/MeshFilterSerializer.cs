using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(MeshFilter), -1)]
    public sealed class MeshFilterSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                DataSerializerUtility.SerializeFieldAsset(nameof(MeshFilter.sharedMesh), obj.sharedMesh, fieldData);
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshFilter;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                if (DataSerializerUtility.DeserializeFieldAsset(nameof(MeshFilter.sharedMesh), out object sharedMesh, typeof(MeshFilter), fieldData))
                    obj.mesh = sharedMesh as Mesh;
        }
    }
}