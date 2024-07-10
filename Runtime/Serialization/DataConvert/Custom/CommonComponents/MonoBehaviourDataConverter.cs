using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MonoBehaviourDataConverter : IDataConverter
    {
        int IDataConverter.ConversionPriority() => -5;
        bool IDataConverter.CanConvert(Type objectType) => typeof(MonoBehaviour).IsAssignableFrom(objectType);

        void IDataConverter.Serialize(Type valueType, object value, IDictionary<string, object> output) => DataConvertUtility.Serialize(value, output);
        object IDataConverter.Deserialize(Type valueType, object existingValue, IDictionary<string, object> input) => DataConvertUtility.Deserialize(existingValue, input);
    }
}