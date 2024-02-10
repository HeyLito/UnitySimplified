using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(BoxCollider), -1)]
    public sealed class BoxColliderSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as BoxCollider;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(BoxCollider.isTrigger)] = obj.isTrigger;
                fieldData[nameof(BoxCollider.center)] = obj.center;
                fieldData[nameof(BoxCollider.size)] = obj.size;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(BoxCollider.sharedMaterial), obj.sharedMaterial, fieldData);
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                fieldData[nameof(BoxCollider.contactOffset)] = obj.contactOffset;
        }
        void IDataSerializable.Deserialize(ref object target, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = target as BoxCollider;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(BoxCollider.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(BoxCollider.center), out object center))
                    obj.center = (Vector3)center;
                if (fieldData.TryGetValue(nameof(BoxCollider.size), out object size))
                    obj.size = (Vector3)size;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(BoxCollider.sharedMaterial), out object sharedMaterial, typeof(PhysicMaterial), fieldData))
                        obj.sharedMaterial = (PhysicMaterial)sharedMaterial;
            }

            if (flags.HasFlag(SerializerFlags.NonSerializedVariable))
                if (fieldData.TryGetValue(nameof(BoxCollider.contactOffset), out object contactOffset))
                    obj.contactOffset = (float)contactOffset;
        }
    }
}