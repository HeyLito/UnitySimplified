using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class CircleCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(CircleCollider2D);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as CircleCollider2D;
            if (obj == null)
                return;

            output[nameof(CircleCollider2D.density)] = obj.density;
            output[nameof(CircleCollider2D.isTrigger)] = obj.isTrigger;
            output[nameof(CircleCollider2D.usedByEffector)] = obj.usedByEffector;
            output[nameof(CircleCollider2D.offset)] = obj.offset;
            output[nameof(CircleCollider2D.radius)] = obj.radius;

#if UNITY_6000_0_OR_NEWER
            output[nameof(CircleCollider2D.compositeOperation)] = obj.compositeOperation;
#else
            output[nameof(CircleCollider2D.usedByComposite)] = obj.usedByComposite;
#endif

            DataConvertUtility.TrySerializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type value, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as CircleCollider2D;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(CircleCollider2D.density), out object density))
                obj.density = (float)density;
            if (input.TryGetValue(nameof(CircleCollider2D.isTrigger), out object isTrigger))
                obj.isTrigger = (bool)isTrigger;
            if (input.TryGetValue(nameof(CircleCollider2D.usedByEffector), out object usedByEffector))
                obj.usedByEffector = (bool)usedByEffector;
            if (input.TryGetValue(nameof(CircleCollider2D.offset), out object offset))
                obj.offset = (Vector2)offset;
            if (input.TryGetValue(nameof(CircleCollider2D.radius), out object radius))
                obj.radius = (float)radius;

#if UNITY_6000_0_OR_NEWER
            if (input.TryGetValue(nameof(CircleCollider2D.compositeOperation), out object compositeOperation))
                obj.compositeOperation = (Collider2D.CompositeOperation)compositeOperation;
#else
            if (input.TryGetValue(nameof(CircleCollider2D.usedByComposite), out object usedByComposite))
                obj.usedByComposite = (bool)usedByComposite;
#endif

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(CircleCollider2D.sharedMaterial), out object sharedMaterial, input))
                obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;

            return obj;
        }
    }
}