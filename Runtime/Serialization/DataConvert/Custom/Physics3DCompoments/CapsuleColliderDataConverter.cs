using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class CapsuleColliderDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(CapsuleCollider);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as CapsuleCollider;
            if (obj == null)
                return;

            output[nameof(CapsuleCollider.isTrigger)] = obj.isTrigger;
            output[nameof(CapsuleCollider.center)] = obj.center;
            output[nameof(CapsuleCollider.radius)] = obj.radius;
            output[nameof(CapsuleCollider.height)] = obj.height;
            output[nameof(CapsuleCollider.direction)] = obj.direction;
            output[nameof(CapsuleCollider.contactOffset)] = obj.contactOffset;

            DataConvertUtility.TrySerializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as CapsuleCollider;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(CapsuleCollider.isTrigger), out object isTrigger))
                obj.isTrigger = (bool)isTrigger;
            if (input.TryGetValue(nameof(CapsuleCollider.center), out object center))
                obj.center = (Vector3)center;
            if (input.TryGetValue(nameof(CapsuleCollider.radius), out object radius))
                obj.radius = (float)radius;
            if (input.TryGetValue(nameof(CapsuleCollider.height), out object height))
                obj.height = (float)height;
            if (input.TryGetValue(nameof(CapsuleCollider.direction), out object direction))
                obj.direction = (int)direction;
            if (input.TryGetValue(nameof(CapsuleCollider.contactOffset), out object contactOffset))
                obj.contactOffset = (float)contactOffset;

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), out object sharedMaterial, input))
#if UNITY_6000_0_OR_NEWER
                obj.sharedMaterial = (PhysicsMaterial)sharedMaterial;
#else
                obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
#endif

            return obj;
        }
    }
}