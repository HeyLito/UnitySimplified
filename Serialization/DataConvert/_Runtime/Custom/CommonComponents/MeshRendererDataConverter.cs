using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MeshRendererDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MeshRenderer);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> ouput, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
                for (int i = 0; i < obj.sharedMaterials.Length; i++)
                    DataConvertUtility.SerializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, obj.sharedMaterials[i], ouput);
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as MeshRenderer;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable) && flags.HasFlag(SerializerFlags.AssetReference))
            {
                List<Material> materials = new List<Material>();
                int i = -1;
                foreach (var pair in input)
                {
                    i++;
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, out object sharedMaterial, typeof(Material), input))
                        materials.Add(sharedMaterial as Material);
                }
                obj.materials = materials.ToArray();
            }
        }
    }
}