using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class SphereColliderDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MonoBehaviour);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as SphereCollider;
            if (obj == null)
                return;

            output[nameof(SphereCollider.isTrigger)] = obj.isTrigger;
            output[nameof(SphereCollider.center)] = obj.center;
            output[nameof(SphereCollider.radius)] = obj.radius;
            output[nameof(SphereCollider.contactOffset)] = obj.contactOffset;

            DataConvertUtility.TrySerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as SphereCollider;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(SphereCollider.isTrigger), out object isTrigger))
                obj.isTrigger = (bool)isTrigger;
            if (input.TryGetValue(nameof(SphereCollider.center), out object center))
                obj.center = (Vector3)center;
            if (input.TryGetValue(nameof(SphereCollider.radius), out object radius))
                obj.radius = (float)radius;
            if (input.TryGetValue(nameof(SphereCollider.contactOffset), out object contactOffset))
                obj.contactOffset = (float)contactOffset;

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(SphereCollider.sharedMaterial), out object sharedMaterial, input))
#if UNITY_6000_0_OR_NEWER
                obj.sharedMaterial = (PhysicsMaterial)sharedMaterial;
#else
                obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
#endif

            return obj;
        }
    }
}