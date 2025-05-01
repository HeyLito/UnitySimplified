using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class BoxCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(MonoBehaviour);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as BoxCollider2D;
            if (obj == null)
                return;

            output[nameof(BoxCollider2D.density)] = obj.density;
            output[nameof(BoxCollider2D.isTrigger)] = obj.isTrigger;
            output[nameof(BoxCollider2D.usedByEffector)] = obj.usedByEffector;
            output[nameof(BoxCollider2D.autoTiling)] = obj.autoTiling;
            output[nameof(BoxCollider2D.offset)] = obj.offset;
            output[nameof(BoxCollider2D.size)] = obj.size;
            output[nameof(BoxCollider2D.edgeRadius)] = obj.edgeRadius;

#if UNITY_6000_0_OR_NEWER
            output[nameof(BoxCollider2D.compositeOperation)] = obj.compositeOperation;
#else
            output[nameof(BoxCollider2D.usedByComposite)] = obj.usedByComposite;
#endif

            DataConvertUtility.TrySerializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as BoxCollider2D;
            if (obj == null)
                return null;

            if (input.TryGetValue(nameof(BoxCollider2D.density), out object density))
                obj.density = (float)density;
            if (input.TryGetValue(nameof(BoxCollider2D.isTrigger), out object isTrigger))
                obj.isTrigger = (bool)isTrigger;
            if (input.TryGetValue(nameof(BoxCollider2D.usedByEffector), out object usedByEffector))
                obj.usedByEffector = (bool)usedByEffector;
            if (input.TryGetValue(nameof(BoxCollider2D.autoTiling), out object autoTiling))
                obj.autoTiling = (bool)autoTiling;
            if (input.TryGetValue(nameof(BoxCollider2D.offset), out object offset))
                obj.offset = (Vector2)offset;
            if (input.TryGetValue(nameof(BoxCollider2D.size), out object size))
                obj.size = (Vector2)size;
            if (input.TryGetValue(nameof(BoxCollider2D.edgeRadius), out object edgeRadius))
                obj.edgeRadius = (float)edgeRadius;

#if UNITY_6000_0_OR_NEWER
            if (input.TryGetValue(nameof(BoxCollider2D.compositeOperation), out object compositeOperation))
                obj.compositeOperation = (Collider2D.CompositeOperation)compositeOperation;
#else
            if (input.TryGetValue(nameof(BoxCollider2D.usedByComposite), out object usedByComposite))
                obj.usedByComposite = (bool)usedByComposite;
#endif

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(BoxCollider2D.sharedMaterial), out object sharedMaterial, input))
                obj.sharedMaterial = (PhysicsMaterial2D)sharedMaterial;

            return obj;
        }
    }
}