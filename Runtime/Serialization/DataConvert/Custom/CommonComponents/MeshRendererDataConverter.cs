using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MeshRendererDataConverter : IDataConverter<MeshRenderer>
    {
        int IDataConverter.ConversionPriority() => -1;

        public void Serialize(MeshRenderer value, IDictionary<string, object> output)
        {
            for (int i = 0; i < value.sharedMaterials.Length; i++)
                DataConvertUtility.TrySerializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, value.sharedMaterials[i], output);
        }
        public MeshRenderer Deserialize(MeshRenderer existingValue, IDictionary<string, object> input)
        {
            List<Material> materials = new();
            for (int i = 0; i < input.Count; i++)
                if (DataConvertUtility.TryDeserializeFieldAsset(nameof(MeshRenderer.sharedMaterials) + i, out object sharedMaterial, input))
                    materials.Add(sharedMaterial as Material);

            existingValue.materials = materials.ToArray();
            return existingValue;
        }
    }
}