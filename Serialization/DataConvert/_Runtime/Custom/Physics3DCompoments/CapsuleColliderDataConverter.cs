using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class CapsuleColliderDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(CapsuleCollider);
        void IDataConverter.Serialize(object target, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = target as CapsuleCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(CapsuleCollider.isTrigger)] = obj.isTrigger;
                output[nameof(CapsuleCollider.center)] = obj.center;
                output[nameof(CapsuleCollider.radius)] = obj.radius;
                output[nameof(CapsuleCollider.height)] = obj.height;
                output[nameof(CapsuleCollider.direction)] = obj.direction;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), obj.sharedMaterial, output);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                output[nameof(CapsuleCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataConverter.Deserialize(ref object target, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = target as CapsuleCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
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

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), input))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (input.TryGetValue(nameof(CapsuleCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}