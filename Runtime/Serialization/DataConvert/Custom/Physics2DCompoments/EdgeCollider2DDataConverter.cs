using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class EdgeCollider2DDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => objectType == typeof(EdgeCollider2D);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as EdgeCollider2D;
            if (obj == null)
                return;

            output[nameof(EdgeCollider2D.density)] = obj.density;
            output[nameof(EdgeCollider2D.isTrigger)] = obj.isTrigger;
            output[nameof(EdgeCollider2D.usedByEffector)] = obj.usedByEffector;
            output[nameof(EdgeCollider2D.usedByComposite)] = obj.usedByComposite;
            output[nameof(EdgeCollider2D.offset)] = obj.offset;
            output[nameof(EdgeCollider2D.edgeRadius)] = obj.edgeRadius;
            output[nameof(EdgeCollider2D.points)] = obj.points;

            DataConvertUtility.TrySerializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), obj.sharedMaterial, output);
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            var obj = existingValue as EdgeCollider2D;
            if (obj == null)
                return null;

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

            if (DataConvertUtility.TryDeserializeFieldAsset(nameof(EdgeCollider2D.sharedMaterial), out object sharedMaterial, input));

            return obj;
        }
    }
}