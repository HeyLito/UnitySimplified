using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    [CustomSerializer(typeof(BoxCollider2D), -1)]
    public sealed class BoxCollider2DSerializer : IDataSerializable
    {
        void IDataSerializable.Serialize(object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                fieldData[nameof(BoxCollider2D.density)] = obj.density;
                fieldData[nameof(BoxCollider2D.isTrigger)] = obj.isTrigger;
                fieldData[nameof(BoxCollider2D.usedByEffector)] = obj.usedByEffector;
                fieldData[nameof(BoxCollider2D.usedByComposite)] = obj.usedByComposite;
                fieldData[nameof(BoxCollider2D.autoTiling)] = obj.autoTiling;
                fieldData[nameof(BoxCollider2D.offset)] = obj.offset;
                fieldData[nameof(BoxCollider2D.size)] = obj.size;
                fieldData[nameof(BoxCollider2D.edgeRadius)] = obj.edgeRadius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataSerializerUtility.SerializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), obj.sharedMaterial, fieldData);
            }
        }
        void IDataSerializable.Deserialize(ref object instance, IDictionary<string, object> fieldData, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (fieldData.TryGetValue(nameof(BoxCollider2D.density), out object density))
                    obj.density = (float)density;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.autoTiling), out object autoTiling))
                    obj.autoTiling = (bool)autoTiling;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.size), out object size))
                    obj.size = (Vector2)size;
                if (fieldData.TryGetValue(nameof(BoxCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataSerializerUtility.DeserializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), fieldData))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}
