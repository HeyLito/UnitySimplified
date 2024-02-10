using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(CapsuleCollider), -1)]
    public sealed class CapsuleColliderSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as CapsuleCollider;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(CapsuleCollider.isTrigger)] = obj.isTrigger;
                fieldData[nameof(CapsuleCollider.center)] = obj.center;
                fieldData[nameof(CapsuleCollider.radius)] = obj.radius;
                fieldData[nameof(CapsuleCollider.height)] = obj.height;
                fieldData[nameof(CapsuleCollider.direction)] = obj.direction;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), obj.sharedMaterial, fieldData);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                fieldData[nameof(CapsuleCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataSerializable.Deserialize(ref object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as CapsuleCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(CapsuleCollider.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(CapsuleCollider.center), out object center))
                    obj.center = (Vector3)center;
                if (fieldData.TryGetValue(nameof(CapsuleCollider.radius), out object radius))
                    obj.radius = (float)radius;
                if (fieldData.TryGetValue(nameof(CapsuleCollider.height), out object height))
                    obj.height = (float)height;
                if (fieldData.TryGetValue(nameof(CapsuleCollider.direction), out object direction))
                    obj.direction = (int)direction;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(CapsuleCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), fieldData))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (fieldData.TryGetValue(nameof(CapsuleCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}