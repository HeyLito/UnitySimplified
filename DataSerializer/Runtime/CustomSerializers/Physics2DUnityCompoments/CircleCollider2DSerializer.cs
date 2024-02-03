using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(CircleCollider2D), -1)]
    public sealed class CircleCollider2DSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(CircleCollider2D.density)] = obj.density;
                fieldData[nameof(CircleCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(CircleCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(CircleCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(CircleCollider2D.offset)] = obj.offset;
                fieldData[nameof(CircleCollider2D.radius)] = obj.radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(CircleCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(CircleCollider2D.radius), out object radius))
                    obj.radius = (float)radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}
