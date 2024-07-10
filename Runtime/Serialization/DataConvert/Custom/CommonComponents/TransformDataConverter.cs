using System;
using System.Collections.Generic;
using UnityEngine;
using UnitySimplified.RuntimeDatabases;

namespace UnitySimplified.Serialization
{
    public sealed class TransformDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -1;
        bool IDataConverter.CanConvert(Type objectType) => typeof(Transform).IsAssignableFrom(objectType);
        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output)
        {
            var obj = value as Transform;
            if (obj == null)
                return;

            if (RuntimeReferenceDatabase.Instance.TryGet(value, out string referenceID))
                output["$referenceID"] = referenceID;

            if (DataConvertUtility.TrySerializeValue(obj.localPosition, out var serializedPosition))
                output["position"] = serializedPosition;
            if (DataConvertUtility.TrySerializeValue(obj.localRotation.eulerAngles, out var serializedRotation))
                output["rotation"] = serializedRotation;
            if (DataConvertUtility.TrySerializeValue(obj.localScale, out var serializedScale))
                output["scale"] = serializedScale;
        }
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input)
        {
            if (input.TryGetValue("$referenceID", out var referenceIDObject) && referenceIDObject is string referenceID)
                if (RuntimeReferenceDatabase.Instance.TryGet(referenceID, out var reference))
                    existingValue = reference;

            var obj = existingValue as Transform;
            if (obj == null)
                return null;

            if (DataConvertUtility.TryDeserializeValue(input["position"], obj.localPosition, out Vector3 position))
                obj.localPosition = position;
            if (DataConvertUtility.TryDeserializeValue(input["rotation"], obj.localRotation.eulerAngles, out Vector3 rotation))
                obj.localRotation = Quaternion.Euler(rotation);
            if (DataConvertUtility.TryDeserializeValue(input["scale"], obj.localScale, out Vector3 scale))
                obj.localScale = scale;

            return obj;
        }
    }
}