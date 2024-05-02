using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitySimplified.Serialization
{
    public sealed class MonoBehaviourDataConverter : IDataConverter
    {
        int IDataConverter.GetConversionPriority() => -5;
        bool IDataConverter.CanConvert(Type objectType) => typeof(MonoBehaviour).IsAssignableFrom(objectType);
        void IDataConverter.Serialize(object instance, IDictionary<string, object> output, SerializerFlags flags) => DataConvertUtility.Serialize(instance, output, flags);
        void IDataConverter.Deserialize(ref object instance, IDictionary<string, object> input, SerializerFlags flags) => DataConvertUtility.Deserialize(instance, input, flags);
    }
}