using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class EdgeCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(EdgeCollider2D);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(EdgeCollider2D.density)] = obj.density;
                output[nameof(EdgeCollider2D.isTrigger)] = obj.isTrigger;
                output[nameof(EdgeCollider2D.usedByEffector)] = obj.usedByEffector;
                output[nameof(EdgeCollider2D.usedByComposite)] = obj.usedByComposite;
                output[nameof(EdgeCollider2D.offset)] = obj.offset;
                output[nameof(EdgeCollider2D.edgeRadius)] = obj.edgeRadius;
                output[nameof(EdgeCollider2D.points)] = obj.points;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), obj.sharedMaterial, output);
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as EdgeCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(EdgeCollider2D.density), out object density))
                    obj.density = (float)density;
                if (input.TryGetValue(nameof(EdgeCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (input.TryGetValue(nameof(EdgeCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (input.TryGetValue(nameof(EdgeCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (input.TryGetValue(nameof(EdgeCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (input.TryGetValue(nameof(EdgeCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;
                if (input.TryGetValue(nameof(EdgeCollider2D.points), out object points))
                    obj.points = (Vector2[])points;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), input))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}