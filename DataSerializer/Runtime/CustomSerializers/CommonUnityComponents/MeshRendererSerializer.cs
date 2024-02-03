using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(MeshRenderer), -1)]
    public sealed class MeshRendererSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                for (int i = 0; i < obj.sharedMaterials.Length; i++)
                    DataSerializerUtility.SerializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, obj.sharedMaterials[i], fieldData);
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
            {
                List<Material> materials = new List<Material>();
                int i = -1;
                foreach (var pair in fieldData)
                {
                    i++;
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, out object sharedMaterial, typeof(Material), fieldData))
                        materials.Add(sharedMaterial as Material);
                }
                obj.materials = materials.ToArray();
            }
        }
    }
}