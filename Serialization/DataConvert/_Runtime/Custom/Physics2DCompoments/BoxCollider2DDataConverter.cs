using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class BoxCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MonoBehaviour);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                output[nameof(BoxCollider2D.density)] = obj.density;
                output[nameof(BoxCollider2D.isTrigger)] = obj.isTrigger;
                output[nameof(BoxCollider2D.usedByEffector)] = obj.usedByEffector;
                output[nameof(BoxCollider2D.usedByComposite)] = obj.usedByComposite;
                output[nameof(BoxCollider2D.autoTiling)] = obj.autoTiling;
                output[nameof(BoxCollider2D.offset)] = obj.offset;
                output[nameof(BoxCollider2D.size)] = obj.size;
                output[nameof(BoxCollider2D.edgeRadius)] = obj.edgeRadius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    DataConvertUtility.SerializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), obj.sharedMaterial, output);
            }
        }
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags)
        {
            var obj = instance as BoxCollider2D;
            if (obj == null)
                return;

            if (flags.HasFlag(SerializerFlags.SerializedVariable))
            {
                if (input.TryGetValue(nameof(BoxCollider2D.density), out object density))
                    obj.density = (float)density;
                if (input.TryGetValue(nameof(BoxCollider2D.isTrigger), out object isTrigger))
                    obj.isTrigger = (bool)isTrigger;
                if (input.TryGetValue(nameof(BoxCollider2D.usedByEffector), out object usedByEffector))
                    obj.usedByEffector = (bool)usedByEffector;
                if (input.TryGetValue(nameof(BoxCollider2D.usedByComposite), out object usedByComposite))
                    obj.usedByComposite = (bool)usedByComposite;
                if (input.TryGetValue(nameof(BoxCollider2D.autoTiling), out object autoTiling))
                    obj.autoTiling = (bool)autoTiling;
                if (input.TryGetValue(nameof(BoxCollider2D.offset), out object offset))
                    obj.offset = (Vector2)offset;
                if (input.TryGetValue(nameof(BoxCollider2D.size), out object size))
                    obj.size = (Vector2)size;
                if (input.TryGetValue(nameof(BoxCollider2D.edgeRadius), out object edgeRadius))
                    obj.edgeRadius = (float)edgeRadius;

                if (flags.HasFlag(SerializerFlags.AssetReference))
                    if (DataConvertUtility.DeserializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), out object sharedMaterial, typeof(PhysicsMaterial2D), input))
                        obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;
            }
        }
    }
}