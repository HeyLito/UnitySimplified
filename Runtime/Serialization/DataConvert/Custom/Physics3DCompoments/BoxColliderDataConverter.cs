using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class BoxColliderDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(BoxCollider);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as BoxCollider;
            if (obj == null)
                return;

            output[nameof(BoxCollider.isTrigger)] = obj.isTrigger;
            output[nameof(BoxCollider.center)] = obj.center;
            output[nameof(BoxCollider.size)] = obj.size;
            output[nameof(BoxCollider.contactOffset)] = obj.contactOffset;

            DataConvertUtility.TrySerializeFieldAsset(nameof(BoxCollider.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as BoxCollider;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(BoxCollider.isTrigger), out object isTrigger))
                obj.isTrigger = (bool)isTrigger;
            if (input.TryGetValue(nameof(BoxCollider.center), out object center))
                obj.center = (Vector3)center;
            if (input.TryGetValue(nameof(BoxCollider.size), out object size))
                obj.size = (Vector3)size;
            if (input.TryGetValue(nameof(BoxCollider.contactOffset), out object contactOffset))
                obj.contactOffset = (float)contactOffset;

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(BoxCollider.sharedMaterial), out object sharedMaterial, input))
#if UNITY_6000_0_OR_NEWER
                obj.sharedMaterial = (PhysicsMaterial)sharedMaterial;
#else
                obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
#endif

            return obj;
        }
    }
}