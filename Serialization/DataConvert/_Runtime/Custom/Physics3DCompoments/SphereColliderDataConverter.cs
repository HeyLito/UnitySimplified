using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class SphereColliderDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MonoBehaviour);
        void IDataConverter.Serialize(object target, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = target as SphereCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(SphereCollider.isTrigger)] = obj.isTrigger;
                output[nameof(SphereCollider.center)] = obj.center;
                output[nameof(SphereCollider.radius)] = obj.radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, output);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                output[nameof(SphereCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataConverter.Deserialize(ref object target, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = target as SphereCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(SphereCollider.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (input.TryGetValue(nameof(SphereCollider.center), out object center))
                    obj.center = (Vector3)center;
                if (input.TryGetValue(nameof(SphereCollider.radius), out object radius))
                    obj.radius = (float)radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(SphereCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), input))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (input.TryGetValue(nameof(SphereCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}