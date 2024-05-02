using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class CircleCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(CircleCollider2D);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(CircleCollider2D.density)] = obj.density;
                output[nameof(CircleCollider2D.isTrigger)] = obj.isTrigger;
                output[nameof(CircleCollider2D.usedByEffector)] = obj.usedByEffector;
                output[nameof(CircleCollider2D.usedByComposite)] = obj.usedByComposite;
                output[nameof(CircleCollider2D.offset)] = obj.offset;
                output[nameof(CircleCollider2D.radius)] = obj.radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, output);
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as CircleCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(CircleCollider2D.density), out object density))
                    obj.density = (float)density;
                if (input.TryGetValue(nameof(CircleCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (input.TryGetValue(nameof(CircleCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (input.TryGetValue(nameof(CircleCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (input.TryGetValue(nameof(CircleCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (input.TryGetValue(nameof(CircleCollider2D.radius), out object radius))
                    obj.radius = (float)radius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), input))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}