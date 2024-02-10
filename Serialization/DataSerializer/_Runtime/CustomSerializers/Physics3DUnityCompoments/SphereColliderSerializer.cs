using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(SphereCollider), -1)]
    public sealed class SphereColliderSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as SphereCollider;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(SphereCollider.isTrigger)] = obj.isTrigger;
                fieldData[nameof(SphereCollider.center)] = obj.center;
                fieldData[nameof(SphereCollider.radius)] = obj.radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                fieldData[nameof(SphereCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataSerializable.Deserialize(ref object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as SphereCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(SphereCollider.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(SphereCollider.center), out object center))
                    obj.center = (Vector3)center;
                if (fieldData.TryGetValue(nameof(SphereCollider.radius), out object radius))
                    obj.radius = (float)radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(SphereCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), fieldData))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (fieldData.TryGetValue(nameof(SphereCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}