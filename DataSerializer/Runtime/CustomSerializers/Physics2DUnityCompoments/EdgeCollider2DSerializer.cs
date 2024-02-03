using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(EdgeCollider2D), -1)]
    public sealed class EdgeCollider2DSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(EdgeCollider2D.density)] = obj.density;
                fieldData[nameof(EdgeCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(EdgeCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(EdgeCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(EdgeCollider2D.offset)] = obj.offset;
                fieldData[nameof(EdgeCollider2D.edgeRadius)] = obj.edgeRadius;
                fieldData[nameof(EdgeCollider2D.points)] = obj.points;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;
                if (fieldData.TryGetValue(nameof(EdgeCollider2D.points), out object points))
                    obj.points = (Vector2[])points;
                
                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}